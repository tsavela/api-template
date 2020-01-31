using System;
using Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Api
{
    public class Program
    {
        public static int Main(string[] args)
        {
            LoggingConfiguration.SetupLogger(
                Environment.GetEnvironmentVariable("LOG_LEVEL"),
                Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY"));

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Give the queue listeners some time to do a graceful shutdown. Default shutdown grace period in Kubernetes is 30 seconds.
                    webBuilder.UseShutdownTimeout(TimeSpan.FromSeconds(30));
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.AddServerHeader = false;
                    });
                })
                .UseSerilog(logger: Log.Logger);
    }
}
