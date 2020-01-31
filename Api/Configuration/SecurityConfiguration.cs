using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Configuration
{
    public static class SecurityConfiguration
    {
        public static IServiceCollection ConfigureSecurity(this IServiceCollection services,
            string azureAdAuthority, string azureAdClientId)
        {
            services.Configure<MvcOptions>(options =>
            {
                // Require authenticated user in *all* calls
                var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.Audience = azureAdClientId;
                option.Authority = azureAdAuthority;
            });

            return services;
        }

        public static IApplicationBuilder ConfigureSecurity(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}
