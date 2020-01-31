using System.Collections.Generic;
using Api.Responses;
using AutoMapper;
using Core.Models;

namespace Api.Mappings
{
    public class WeatherForecastMappings : Profile
    {
        public WeatherForecastMappings()
        {
            CreateMap<IEnumerable<WeatherForecast>, WeatherForecastResponseV1>()
                .ForMember(dest => dest.Forecasts, opts => opts.MapFrom(src => src));
            CreateMap<WeatherForecast, WeatherForecastV1>();
        }
    }
}
