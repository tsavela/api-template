using System;
using Core.Models;
using Core.OutboundPorts;

namespace Database
{
    public class WeatherForecastRepository : IWeatherForecastRepository
    {
        private static readonly string[] Summaries = {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecast GetWeatherForecast(int daysFromNow)
        {
            var rng = new Random();

            return new WeatherForecast
            {
                DaysFromNow = daysFromNow,
                TemperatureC = daysFromNow * 5,
                Summary = Summaries[daysFromNow - 1]
            };
        }
    }
}
