using System;
using System.Collections.Generic;
using System.Linq;
using Core.Models;
using Core.OutboundPorts;
using Serilog;

namespace Core.InboundPorts
{
    public class WeatherForecastService
    {
        private readonly ILogger _logger;
        private readonly IWeatherForecastRepository _weatherForecastRepository;

        public WeatherForecastService(ILogger logger, IWeatherForecastRepository weatherForecastRepository)
        {
            _logger = logger;
            _weatherForecastRepository = weatherForecastRepository;
        }

        public IEnumerable<WeatherForecast> GetWeatherForecast(int forecastLengthInDays)
        {
            return Enumerable.Range(1, forecastLengthInDays).Select(index => {
                    _logger.Debug("In loop step {index}", index);

                    return _weatherForecastRepository.GetWeatherForecast(index);
                })
                .ToArray();
        }
    }
}
