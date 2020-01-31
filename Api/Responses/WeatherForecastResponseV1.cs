using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Api.Configuration;

namespace Api.Responses
{
    public class WeatherForecastResponseV1
    {
        /// <summary>
        /// The weather forecasts.
        /// </summary>
        public IEnumerable<WeatherForecastV1> Forecasts { get; set; } = new WeatherForecastV1[0];
    }

    public class WeatherForecastV1
    {
        /// <summary>
        /// Defines the number of days from now for this forecast.
        /// </summary>
        [Required]
        public int DaysFromNow { get; set; }

        /// <summary>
        /// Temperature in Celsius for this forecast.
        /// </summary>
        [Required]
        public int TemperatureC { get; set; }

        /// <summary>
        /// Textual summary of this forecast.
        /// </summary>
        [Required]
        public string Summary { get; set; } = string.Empty;
    }

    public class WeatherForecastResponseV1Example : IExampleProvider
    {
        public object GetExample()
        {
            return new WeatherForecastResponseV1
            {
                Forecasts = new[]
                {
                    new WeatherForecastV1 {DaysFromNow = 0, Summary = "Freezing", TemperatureC = 0},
                    new WeatherForecastV1 {DaysFromNow = 1, Summary = "Hot", TemperatureC = 100}
                }
            };
        }
    }
}
