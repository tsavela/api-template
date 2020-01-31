using System;
using Api.Tests.Setup;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Api.Tests
{
    public class ApiTestBase : IDisposable
    {
        protected readonly ITestOutputHelper Output;
        protected readonly TestServerBuilder TestServerBuilder;
        protected readonly TestServer TestServer;

        public ApiTestBase(ITestOutputHelper output, bool disableAuth = true)
        {
            Output = output;
            TestServerBuilder = new TestServerBuilder(Output);
            TestServer = TestServerBuilder.Build(disableAuth);
        }

        public void Dispose()
        {
            TestServer.Host.StopAsync().Wait();
            TestServer.Dispose();
        }
    }
}
