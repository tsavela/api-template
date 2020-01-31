using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.Tests.Setup;
using ApprovalTests;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests.ApiContracts
{
    /// <summary>
    /// The purpose with these tests are to assert that the API contract is still in place,
    /// not to break the API for any consumers.
    /// </summary>
    public class WeatherForecastControllerV1Tests
    {
        private readonly TestServer _testServer;

        public WeatherForecastControllerV1Tests(ITestOutputHelper output)
        {
            _testServer = new TestServerBuilder(output).Build();
        }

        [Fact]
        public async Task GetWeatherForecast_ShouldReturnJsonAccordingToContract()
        {
            // Important to use a fixed json string and not serialize a request type, to avoid accidentally
            // changing the request when refacting, breaking the contract without noticing.
            var body = "{\"forecastLengthInDays\": 3}";
            var client = _testServer.CreateClient();

            var actual = await client.PostAsync("/api/v1/WeatherForecast",
                new StringContent(body, Encoding.UTF8, "application/json"));

            actual.StatusCode.Should().Be(HttpStatusCode.OK);

            Approvals.Verify((await actual.Content.ReadAsStringAsync()).PrettifyJson());
        }
    }
}
