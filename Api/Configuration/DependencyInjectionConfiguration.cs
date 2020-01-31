using System.Linq;
using System.Net.Http;
using System.Reflection;
using Core.Exceptions;
using Database;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Configuration
{
    public static class DependencyInjectionConfiguration
    {
        public static IServiceCollection ConfigureDependencyInjection(this IServiceCollection services)
        {
            services.Scan(scan => scan
                // Add all assemblies containing dependencies here
                .FromAssemblies(DependencyAssemblies)
                // Register types that implements an interface as the implemented interfaces
                // Any type that expects a HttpClient dependency will not registered here but handled using .AddHttpClient<> below.
                .AddClasses(classes => classes.Where(c =>
                    c.GetInterfaces().Any(i => i.Namespace != null && i.Namespace.StartsWith("Core") &&
                                               !i.Namespace.StartsWith("Core.Models"))
                    && !c.GetConstructors().Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(HttpClient)))))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                // Add Core inbound ports (they do not inherit from an interface)
                .AddClasses(classes => classes.InNamespaces("Core.InboundPorts"))
                .AsSelf()
                .WithScopedLifetime()
            );

            return services;
        }

        private static Assembly[] DependencyAssemblies
        {
            get
            {
                return new[]
                {
                    typeof(DomainException).Assembly,
                    typeof(DataContext).Assembly
                };
            }
        }
    }
}
