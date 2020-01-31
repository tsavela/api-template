using System.Reflection;
using AutoMapper;
using Core.InboundPorts;
using Database;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Configuration
{
    public static class AutoMapperConfiguration
    {
        public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services)
        {
            // Register all AutoMapper profiles from the below assemblies
            services.AddAutoMapper(
                Assembly.GetExecutingAssembly(),
                typeof(WeatherForecastService).Assembly,
                typeof(DataContext).Assembly);

            return services;
        }
    }
}
