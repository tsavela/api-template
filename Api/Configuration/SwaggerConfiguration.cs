using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using NSwag.Generation.Processors.Security;

namespace Api.Configuration
{
    // ReSharper disable ClassNeverInstantiated.Local

    public static class SwaggerConfiguration
    {
        public static IServiceCollection ConfigureSwagger(this IServiceCollection services,
            string azureAdAuthority, string azureAdClientId, string apiName)
        {
            if (apiName == null) throw new ArgumentNullException(nameof(apiName));

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "VVV";
                // Will remove the version tags from the ApiExplorer routes. This will generate cleaner routes in Swagger docs.
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddOpenApiDocument(document =>
            {
                document.Title = apiName;
                document.Description = "TODO";

                document.Version = "v1";
                document.DocumentName = "v1";
                document.ApiGroupNames = new[] {"1"};

                var authFlow = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = $"{azureAdAuthority}/oauth2/authorize?resource={azureAdClientId}",
                    TokenUrl = $"{azureAdAuthority}/oauth2/token",
                    Scopes = {{"openid", "openid"}}
                };
                document.AddSecurity("bearer", Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.OAuth2,
                        Description = "Azure AD bearer token",
                        Flow = OpenApiOAuth2Flow.Implicit,
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Flows = new OpenApiOAuthFlows() {ClientCredentials = authFlow, Implicit = authFlow}
                    });
                document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
            });
            return services;
        }

        public static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder app, string azureAdClientId, string pathPrefix)
        {
            app.UseOpenApi();
            app.UseSwaggerUi3(settings =>
            {
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    ClientId = azureAdClientId, AppName = azureAdClientId
                };

                var documentPath = settings.DocumentPath;
                settings.DocumentPath = $"/{pathPrefix}{documentPath}";
            });
            return app;
        }
    }

    public class SwaggerExampleProcessor : IOperationProcessor
    {
        private readonly Type _requestExampleType;
        private readonly Type _responseExampleType;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.Indented
        };

        public SwaggerExampleProcessor(Type requestExampleType, Type responseExampleType)
        {
            _requestExampleType = requestExampleType;
            _responseExampleType = responseExampleType;
        }

        public bool Process(OperationProcessorContext context)
        {
            var requestExample = ((IExampleProvider?)Activator.CreateInstance(_requestExampleType))!.GetExample();
            var operationBody = context.OperationDescription.Operation.RequestBody;
            foreach (var bodyType in operationBody.Content.Values)
            {
                bodyType.Example = JsonConvert.SerializeObject(requestExample, JsonSerializerSettings);
            }

            var responseExample = ((IExampleProvider?)Activator.CreateInstance(_responseExampleType))!.GetExample();
            var operationResponse = context.OperationDescription.Operation.ActualResponses["200"];
            operationResponse.Examples = JsonConvert.SerializeObject(responseExample, JsonSerializerSettings);

            return true;
        }
    }

    public interface IExampleProvider
    {
        object GetExample();
    }
}
