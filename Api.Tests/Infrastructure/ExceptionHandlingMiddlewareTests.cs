using System;
using System.Net;
using System.Threading.Tasks;
using Api.Tests.Setup;
using ApprovalTests;
using Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Api.Tests.Infrastructure
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly TestServer _testServer;

        public ExceptionHandlingMiddlewareTests(ITestOutputHelper output)
        {
            _testServer = new TestServerBuilder(output).Build();
        }

        [Fact]
        public async Task ExceptionHandlingMiddleware_ShouldGiveErrorJsonAccordingToContract_GivenThrownException()
        {
            var sut = _testServer.CreateRequest("api/v1/exceptions/exception");
            var actual = await sut.GetAsync();

            actual.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var json = await actual.Content.ReadAsStringAsync();
            Approvals.Verify(json);
        }

        [Fact]
        public async Task ExceptionHandlingMiddleware_ShouldGiveInformativeErrorJson_GivenThrownDomainException()
        {
            var sut = _testServer.CreateRequest("api/v1/exceptions/core");
            var actual = await sut.GetAsync();

            actual.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = await actual.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<ProblemDetails>(json);
            content.Title.Should().Be("Business rule violation");
            content.Detail.Should().Be("Friendly message");
        }

        [Fact]
        public async Task ExceptionHandlingMiddleware_ShouldGiveInformativeErrorJsonWithGenericErrorMessage_GivenThrownExceptionNotSpecificallyHandled()
        {
            var sut = _testServer.CreateRequest("api/v1/exceptions/exception");
            var actual = await sut.GetAsync();

            actual.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            var json = await actual.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<ProblemDetails>(json);
            content.Title.Should().Be("Unhandled application error");
            content.Detail.Should().Be("Internal server error");
        }
    }

    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/exceptions")]
    public class ExceptionController : ControllerBase
    {
        [HttpGet("core")]
        public void DomainException()
        {
            throw new DomainException("Friendly message");
        }

        [HttpGet("exception")]
        public void Exception()
        {
            throw new Exception("Potentially secret message");
        }
    }
}
