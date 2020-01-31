using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests.Infrastructure
{
    /// <summary>
    /// Check that the /health endpoint is working as expected.
    /// </summary>
    public class HealthTests : ApiTestBase
    {
        public HealthTests(ITestOutputHelper output) : base(output, false)
        {
        }

        [Fact]
        public async Task ReadinessEndpoint_ShouldBeHealthy()
        {
            var sut = TestServer.CreateRequest("/health/ready");
            var actual = await sut.GetAsync();

            actual.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await actual.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }

        [Fact]
        public async Task LivelinessEndpoint_ShouldBeHealthy()
        {
            var sut = TestServer.CreateRequest("/health/live");
            var actual = await sut.GetAsync();

            actual.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await actual.Content.ReadAsStringAsync();
            content.Should().Be("Healthy");
        }
    }
}
