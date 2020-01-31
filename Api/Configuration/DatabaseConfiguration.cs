using System;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Api.Configuration
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, string dbConnectionString)
        {
            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<DataContext>(
                    options => options.UseNpgsql(dbConnectionString,
                    options => options.MigrationsAssembly("Database"))
                        .UseSnakeCaseNamingConvention());

            return services;
        }

        public static IApplicationBuilder ConfigureDatabase(this IApplicationBuilder app)
        {
            app.UseMiddleware<TransactionPerRequestMiddleware<DataContext>>();

            // Need to create a scope to be able to resolve the scoped context
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var database = scope.ServiceProvider.GetRequiredService<DataContext>().Database;
                if (database.IsNpgsql())
                {
                    // Only migrate if the database is an actual Postgres instance (not EF in-memory)
                    database.Migrate();
                }
            }

            return app;
        }

        private class TransactionPerRequestMiddleware<TContext>
            where TContext : DbContext, IDisposable
        {

            private readonly RequestDelegate _next;

            public TransactionPerRequestMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            // ReSharper disable once UnusedMember.Local
            public async Task Invoke(HttpContext httpContext, TContext dbContext, ILogger logger)
            {
                try
                {
                    dbContext.Database.BeginTransaction();
                    await _next.Invoke(httpContext);

                    if (httpContext.Response.IsSuccess())
                    {
                        dbContext.Database.CommitTransaction();
                    }
                    else
                    {
                        dbContext.Database.RollbackTransaction();
                    }
                }
                catch
                {
                    if (dbContext.Database.CurrentTransaction != null)
                        dbContext.Database.RollbackTransaction();

                    throw;
                }
            }
        }
    }

    public static class HttpResponseExtensions
    {
        public static bool IsSuccess(this HttpResponse response)
        {
            var statusCode = response.StatusCode;
            return statusCode >= 200 && statusCode <= 299;
        }
    }
}
