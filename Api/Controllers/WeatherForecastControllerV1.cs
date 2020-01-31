using System.Collections.Generic;
using Api.Requests;
using Api.Responses;
using AutoMapper;
using Core.InboundPorts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/WeatherForecast")]
    public class WeatherForecastControllerV1 : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly WeatherForecastService _weatherForecastService;

        public WeatherForecastControllerV1(IMapper mapper, WeatherForecastService weatherForecastService)
        {
            _mapper = mapper;
            _weatherForecastService = weatherForecastService;
        }

        /// <summary>
        /// Get a faked weather forecast.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public WeatherForecastResponseV1 GetWeatherForecast([FromBody] WeatherForecastRequestV1 request)
        {
            var result = _weatherForecastService.GetWeatherForecast(request.ForecastLengthInDays);
            return _mapper.Map<WeatherForecastResponseV1>(result);
        }
    }
}
