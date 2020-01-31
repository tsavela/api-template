using System;

namespace Core.Models
{
    public class WeatherForecast
    {
        public int DaysFromNow { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
