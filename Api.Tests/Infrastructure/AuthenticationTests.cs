using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Api.Tests.Setup;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests.Infrastructure
{
    /// <summary>
    /// Test that the authentication is in place.
    /// </summary>
    public class AuthenticationTests
    {
        private readonly TestServer _testServer;

        public AuthenticationTests(ITestOutputHelper output)
        {
            // Do not disable auth in these tests
            _testServer = new TestServerBuilder(output).Build(false);
        }

        public async Task ApiEndpoints_ShouldReturnUnauthorized_WhenNotPassingAuthorizationToken()
        {
            // Enumerate all existing endpoints and see that they all return 401
            var apiDescription = _testServer.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>();

            foreach (var description in apiDescription.ApiDescriptionGroups.Items.SelectMany(g => g.Items))
            {
                var sut = _testServer.CreateRequest(description.RelativePath.Replace("{version}", "1"));
                var actual = await sut.SendAsync(description.HttpMethod);
                actual.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }
    }
}
