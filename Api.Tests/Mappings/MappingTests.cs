using Api.Mappings;
using AutoMapper;
using Xunit;

namespace Api.Tests.Mappings
{
    /// <summary>
    /// Test that the AutoMapper mappings are setup correctly.
    /// </summary>
    public class MappingTests
    {
        [Fact]
        public void WeatherForecastMappings_ShouldMapCorrectly()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<WeatherForecastMappings>());
            config.AssertConfigurationIsValid();
        }
    }
}
