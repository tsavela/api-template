using Core.Models;

namespace Core.OutboundPorts
{
    public interface IWeatherForecastRepository
    {
        WeatherForecast GetWeatherForecast(int daysFromNow);
    }
}
