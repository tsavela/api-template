using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;

namespace Api.Configuration
{
    public static class LoggingConfiguration
    {
        private static readonly bool IsDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        private static readonly string ApplicationVersion = GetApplicationVersion();

        public static IServiceCollection ConfigureLogging(this IServiceCollection services, string? aiInstrumentationKey, string? aiAuthenticationApiKey)
        {
            EnableApplicationInsights(services, aiInstrumentationKey, aiAuthenticationApiKey);
            return services;
        }

        private static void EnableApplicationInsights(IServiceCollection services, string? aiInstrumentationKey, string? AiAuthenticationApiKey)
        {
            if (aiInstrumentationKey == null) return;
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.InstrumentationKey = aiInstrumentationKey;
                options.ApplicationVersion = ApplicationVersion;
            });
            services.AddApplicationInsightsTelemetryProcessor<FilterSuccessfulHealthChecks>();
            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, o) => module.AuthenticationApiKey = AiAuthenticationApiKey);
            services.AddApplicationInsightsKubernetesEnricher();
        }

        public static void SetupLogger(string? logLevel, string? aiInstrumentationKey)
        {
            // Parse the requested logging level. Note that specific log sources might have higher log level by default, to avoid unnecessary logs.
            var minimumLogEventLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), logLevel ?? "Information");
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLogEventLevel)
                .MinimumLevel.Override("Microsoft", GetLogEventLevel(LogEventLevel.Information, minimumLogEventLevel))
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", GetLogEventLevel(LogEventLevel.Warning, minimumLogEventLevel))
                .MinimumLevel.Override("Microsoft.AspNetCore.Authorization", GetLogEventLevel(LogEventLevel.Warning, minimumLogEventLevel))
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", GetLogEventLevel(LogEventLevel.Warning, minimumLogEventLevel))
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", GetLogEventLevel(LogEventLevel.Warning, minimumLogEventLevel))
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing", GetLogEventLevel(LogEventLevel.Warning, minimumLogEventLevel))
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", minimumLogEventLevel)
                .Filter.ByExcluding(IsSuccessfulLivenessHealthCheckCall)
                .Enrich.FromLogContext()
                .Enrich.With(new TraceIdEnricher());

            if (IsDevelopment)
            {
                // Keep logs to more human-readable format during development
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Console()
                    .WriteTo.Debug();
            }
            else
            {
                // Use Elastic Search json format in real environments, to be picked up by fluentd
                loggerConfiguration = loggerConfiguration
                    .WriteTo.Console(new ElasticsearchJsonFormatter());
            }

            if (aiInstrumentationKey != null)
            {
                // Push the logs to Application Insights
                loggerConfiguration = loggerConfiguration
                    .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces,
                        minimumLogEventLevel);
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            static LogEventLevel GetLogEventLevel(LogEventLevel requested, LogEventLevel minimum)
            {
                return requested.CompareTo(minimum) <= 0 ? minimum : requested;
            }
        }

        static string GetApplicationVersion()
        {
            if (IsDevelopment) return "Development";

            var metadata = GetAssemblyMetadata();
            var gitHash = metadata.GitHash;
            return $"{gitHash}";
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        public class Metadata
        {
            public string GitHash { get; set; } = string.Empty;
        }

        private static Metadata GetAssemblyMetadata()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = $"{assembly.GetName().Name}.AssemblyMetadata.json";
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null) throw new MissingManifestResourceException("Missing AssemblyMetadata.json as embedded resource");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<Metadata>(json);
        }

        private static bool IsSuccessfulLivenessHealthCheckCall(LogEvent arg)
        {
            // Identify Serilog log events from successful health checks
            return arg.Level <= LogEventLevel.Information &&
                   arg.Properties.TryGetValue("RequestPath", out var requestPath) &&
                   requestPath.ToString().StartsWith("\"/health/live\"", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Adds TraceId from Activity.Current to the log properties.
        /// </summary>
        public class TraceIdEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                if (Activity.Current == null) return;

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", Activity.Current.TraceId.ToString()));
            }
        }

        public class ApplicationVersionTelemetryInitializer : ITelemetryInitializer
        {
            public void Initialize(ITelemetry telemetry)
            {
                if (ApplicationVersion != null) telemetry.Context.Component.Version = ApplicationVersion;
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class FilterSuccessfulHealthChecks : ITelemetryProcessor
        {
            private readonly ITelemetryProcessor _next;

            public FilterSuccessfulHealthChecks(ITelemetryProcessor next)
            {
                _next = next;
            }

            public void Process(ITelemetry item)
            {
                if (item is RequestTelemetry requestTelemetry)
                {
                    // Filter out (do not send) successful request telemetry for the health endpoint to Application Insights
                    if(requestTelemetry.Url.LocalPath.StartsWith("/health", StringComparison.InvariantCultureIgnoreCase) &&
                       requestTelemetry.Success == true)
                        return;
                }

                _next.Process(item);
            }
        }
    }
}
