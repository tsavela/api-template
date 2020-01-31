using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Api.Configuration;
using Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace Api.Tests.Setup
{
    public class TestServerBuilder
    {
        private readonly ITestOutputHelper _xUnitOutput;
        private readonly IWebHostBuilder _builder;

        public readonly DbContextOptions<DataContext> DbContextOptions = new DbContextOptionsBuilder<DataContext>()
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        public TestServerBuilder(ITestOutputHelper xUnitOutput)
        {
            _xUnitOutput = xUnitOutput;
            _builder = new WebHostBuilder();
        }

        public TestServer Build(bool disableAuth = true)
        {
            _builder.ConfigureTestServices(services =>
            {
                if (disableAuth) DisableAuthentication(services);

                // Use in-memory db
                services.AddScoped(_ => new DataContext(DbContextOptions));

                ReplaceLogger(services);

                services.TryAddSingleton<IApiDescriptionGroupCollectionProvider, ApiDescriptionGroupCollectionProvider>();
                services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, DefaultApiDescriptionProvider>());

                services.AddControllers().AddApplicationPart(Assembly.GetExecutingAssembly());

            });

            _builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddInMemoryCollection(GetConfigurationParameters());
            });

            return new TestServer(_builder.UseStartup<Startup>());
        }

        private void DisableAuthentication(IServiceCollection services)
        {
            services.Configure<MvcOptions>(options =>
            {
                // Remove all authorization filters
                while (options.Filters.Any(f => f is AuthorizeFilter))
                {
                    options.Filters.Remove(options.Filters.First(f => f is AuthorizeFilter));
                }
            });
        }

        private void ReplaceLogger(IServiceCollection services)
        {
            // Log to console as text for readable test results
            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Debug)
                .WriteTo.Xunit(_xUnitOutput)
                .WriteTo.Console()
                .WriteTo.Debug()
                .Enrich.With(new LoggingConfiguration.TraceIdEnricher())
                .CreateLogger();
            Log.Logger = logger;
            services.Replace(new ServiceDescriptor(typeof(ILogger), logger));
        }

        public List<KeyValuePair<string, string>> GetConfigurationParameters()
        {
            return new List<KeyValuePair<string, string>>
            {
                // Set required environment variables to be able to run the system in test mode
                new KeyValuePair<string, string>("BACKGROUND_SERVICE_STARTUP_DELAY_SECONDS", "0"),
                new KeyValuePair<string, string>("SHUTDOWN_DELAY_SECONDS", "0"),
                new KeyValuePair<string, string>("AZURE_AD_AUTHORITY", "https://dummyUrl/"),
                new KeyValuePair<string, string>("AZURE_AD_CLIENT_ID", "dummyId"),
                new KeyValuePair<string, string>("DB_CONNECTION_STRING", "dummyString"),
                new KeyValuePair<string, string>("SERVICE_NAME", "TestService"),
                new KeyValuePair<string, string>("API_NAME", "Test Service API")
            };
        }
    }
}
