using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api
{
    public class Startup
    {
        private readonly string[] allowedOrigins = {"http://localhost:3000"};
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.DefaultApiVersion = new ApiVersion(1, 0);
            });

            var azureAdClientId = Configuration["AZURE_AD_CLIENT_ID"];
            var azureAdAuthority = Configuration["AZURE_AD_AUTHORITY"];
            var aiInstrumentationKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            var aiAuthenticationApiKey = Configuration["APPINSIGHTS_QUICKPULSEAUTHAPIKEY"];
            var serviceName = Configuration["SERVICE_NAME"];
            var apiName = Configuration["API_NAME"];

            services
                .ConfigureSecurity(azureAdAuthority, azureAdClientId)
                .ConfigureDependencyInjection()
                .ConfigureDatabase(Configuration["DB_CONNECTION_STRING"])
                .ConfigureHealthChecks(serviceName)
                .ConfigureSwagger(azureAdAuthority, azureAdClientId, apiName)
                .ConfigureAutoMapper()
                .ConfigureLogging(aiInstrumentationKey, aiAuthenticationApiKey)
                .ConfigureMetrics(aiInstrumentationKey, serviceName);

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
        {
            app.UseResponseCompression();
            app.UseRequestResponseLogging();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseRouting();
            app.ConfigureHosting(hostApplicationLifetime, Configuration)
                .ConfigureSwagger(Configuration["AZURE_AD_CLIENT_ID"], Configuration["API_PATH_PREFIX"])
                .ConfigureSecurity()
                .ConfigureDatabase()
                .ConfigureMetrics();

            app.UseEndpoints(endpoints =>
            {
                endpoints.ConfigureHealthChecks();
                endpoints.MapControllers();
            });
        }
    }
}
