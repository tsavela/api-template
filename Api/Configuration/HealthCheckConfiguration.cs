using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Gauge;
using Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Configuration
{
    public static class HealthChecksConfiguration
    {
        private const string ReadinessCheckTag = "ready";
        private const string LivelinessCheckTag = "live";

        public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services, string serviceName)
        {
            // For more information about the reasoning around health checks, see
            // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
            // especially the chapter about "Separate readiness and liveliness probes".

            // Register all IHealthCheck implementations here
            //services.AddSingleton<SomeHealthCheck>();

            // Register and configure all IHealthCheckPublisher implementations
            services.AddSingleton<IHealthCheckPublisher, HealthMetricsPublisher>();
            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                // Do not execute the readiness health checks, since they are only relevant on service startup.
                options.Predicate = check => !check.Tags.Contains(ReadinessCheckTag);
                options.Period = TimeSpan.FromSeconds(60);
            });

            var healthChecksBuilder = services.AddHealthChecks();
            AddReadinessHealthChecks(healthChecksBuilder, serviceName);
            AddLivelinessHealthChecks(healthChecksBuilder, serviceName);
            AddGenericHealthChecks(healthChecksBuilder, serviceName);

            return services;
        }

        private static void AddReadinessHealthChecks(IHealthChecksBuilder healthChecksBuilder, string serviceName)
        {
            // These checks will only be executed when the readiness check endpoint is being called,
            // normally when Kubernetes is starting the pod. Kubernetes will not start to route traffic
            // to the pod until all readiness checks are successful.
            // All readiness checks should have the ReadinessCheckTag tag.
            //healthChecksBuilder.AddCheck<SomeReadinessCheck>($"{serviceName} Queue connectivity", tags: new[] {ReadinessCheckTag});
        }

        private static void AddLivelinessHealthChecks(IHealthChecksBuilder healthChecksBuilder, string serviceName)
        {
            // These checks will be executed when the liveliness check endpoint is being called,
            // used by Kubernetes to determine if the pod is still alive. If not healthy, the pod will be restarted.
            // They will also be executed by the health metrics reporter.
            // All readiness checks should have the LivelinessCheckTag tag.
            // NOTE: The check that Kestrel is alive and can receive requests is handled automatically.
        }

        private static void AddGenericHealthChecks(IHealthChecksBuilder healthChecksBuilder, string serviceName)
        {
            // Generic health checks, these will only be executed by the health metrics reporter.
            healthChecksBuilder.AddDbContextCheck<DataContext>($"{serviceName} database connectivity");
        }

        public static IEndpointRouteBuilder ConfigureHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            // This endpoint will only be called during pod startup, for Kubernetes to decide when the pod is ready to start receiving traffic.
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(ReadinessCheckTag) // Only run readiness health checks
            });

            // This endpoint will be called during the entire pod lifetime, to check if the pod are still alive.
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(LivelinessCheckTag) // Only run liveliness health checks
            });

            endpoints.MapGet("/health", async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "text/plain; charset=UTF-8";
                await context.Response.WriteAsync("Not found. Did you mean to call /health/live or /health/ready?");
            });

            return endpoints;
        }

        /// <summary>
        /// Reports health check status to the metrics provider.
        /// </summary>
        public class HealthMetricsPublisher : IHealthCheckPublisher
        {
            private readonly IMetrics _metrics;

            public HealthMetricsPublisher(IMetrics metrics)
            {
                _metrics = metrics;
            }

            public async Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
            {
                var gaugeOptions = new GaugeOptions
                {
                    Context = "application.health",
                    MeasurementUnit = Unit.None
                };

                foreach (var entry in report.Entries)
                {
                    gaugeOptions.Name = entry.Key;
                    _metrics.Measure.Gauge.SetValue(gaugeOptions, GetMetricValue(entry.Value.Status));
                }
            }

            private static double GetMetricValue(HealthStatus entry) => entry switch
            {
                HealthStatus.Healthy => 1.0,
                HealthStatus.Degraded => 0.5,
                HealthStatus.Unhealthy => 0.0
            };
        }
    }
}
