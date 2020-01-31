using System;
using App.Metrics;
using App.Metrics.Formatters.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Configuration
{
    public static class MetricsConfiguration
    {
        public static IServiceCollection ConfigureMetrics(this IServiceCollection services, string? aiInstrumentationKey, string serviceName)
        {
            if (serviceName == null) throw new ArgumentNullException(nameof(serviceName));

            services
                .AddMetrics(builder =>
                {
                    builder.Configuration.Configure(options =>
                    {
                        options.AddAppTag(serviceName);
                    });

                    if (aiInstrumentationKey != null)
                    {
                        builder.Report.ToApplicationInsights(options =>
                        {
                            options.FlushInterval = TimeSpan.FromSeconds(60);
                            options.InstrumentationKey = aiInstrumentationKey;
                        });
                    }
                    else
                    {
                        builder.Report.ToConsole(options =>
                        {
                            options.FlushInterval = TimeSpan.FromSeconds(60);
                            options.MetricsOutputFormatter =
                                new MetricsJsonOutputFormatter(); // TODO: Change to logstash compatible formatter
                        });
                    }
                })
                .AddMetricsTrackingMiddleware(options =>
                {
                    options.IgnoredRoutesRegexPatterns.Add("health");
                    options.IgnoredRoutesRegexPatterns.Add("metrics");
                    options.ApdexTSeconds = 0.1; // 100 ms
                })
                .AddMetricsReportingHostedService();

            return services;
        }

        public static IApplicationBuilder ConfigureMetrics(this IApplicationBuilder app)
        {
            app.UseMetricsApdexTrackingMiddleware();
            return app;
        }
    }
}
