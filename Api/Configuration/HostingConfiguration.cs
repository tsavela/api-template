using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Api.Configuration
{
    public static class HostingConfiguration
    {
        /// <summary>
        /// Configure hosting-related behaviour.
        /// </summary>
        public static IApplicationBuilder ConfigureHosting(this IApplicationBuilder app,
            IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration)
        {
            hostApplicationLifetime.ApplicationStopping.Register(() =>
            {
                // See https://blog.markvincze.com/graceful-termination-in-kubernetes-with-asp-net-core/
                var delay = TimeSpan.FromSeconds(int.Parse(configuration["SHUTDOWN_DELAY_SECONDS"] ?? "15"));
                Log.Logger.Information($"Shutdown requested. Delaying for {delay} to allow infrastructure to redirect to new service version.");
                Thread.Sleep(delay);
            });

            return app;
        }
    }
}
