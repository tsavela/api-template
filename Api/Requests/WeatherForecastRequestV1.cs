using System.ComponentModel.DataAnnotations;
using Api.Configuration;

namespace Api.Requests
{
    public class WeatherForecastRequestV1
    {
        /// <summary>
        /// Requested number of days in the forecast.
        /// </summary>
        [Required]
        public int ForecastLengthInDays { get; set; }
    }

    public class WeatherForecastRequestV1Example : IExampleProvider
    {
        public object GetExample()
        {
            return new WeatherForecastRequestV1
            {
                ForecastLengthInDays = 5
            };
        }
    }
}
