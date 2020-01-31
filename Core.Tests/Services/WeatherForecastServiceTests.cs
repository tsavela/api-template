using AutoFixture.Xunit2;
using Core.InboundPorts;
using Core.Models;
using Core.OutboundPorts;
using FluentAssertions;
using Serilog.Core;
using Moq;
using Xunit;

namespace Core.Tests.Services
{
    /// <summary>
    /// Test that Core implements the business logic as expected.
    /// </summary>
    public class WeatherForecastServiceTests
    {
        [Theory]
        [AutoData]
        public void WeatherForecastService_ShouldReturnRequestedNumberOfForecasts_WhenWeatherForecastRepositoryReturnsData(int forecastLengthInDays)
        {
            var weatherForecastRepositoryMock = new Mock<IWeatherForecastRepository>();
            weatherForecastRepositoryMock.Setup(m => m.GetWeatherForecast(It.IsAny<int>()))
                .Returns(new WeatherForecast());
            var sut = new WeatherForecastService(Logger.None, weatherForecastRepositoryMock.Object);

            var actual = sut.GetWeatherForecast(forecastLengthInDays);

            actual.Should()
                .NotBeEmpty()
                .And.HaveCount(forecastLengthInDays);
        }
    }
}
