using Microsoft.Extensions.Logging.Abstractions;
using Rochas.Net.Connectivity;

namespace Rochas.RESTClient.Tests
{
    public record ResPost(int Id);
    public record ResFail(int Id);
    public record ResRetryOff(int Id);
    public record ResRetrySuccess(int Id);
    public record ResDelay(int Id);
    public record ResError(int Id);
    public record ResPut(int Id);
    public record ResPatch(int Id);
    public record ResDelete(int Id);

    public class ResilienceTests
    {
        [Fact]
        public async Task Post_WithResilience_OnSuccess_ReturnsTrue()
        {
            var server = new TestHttpServer();
            var client = new RESTClient<ResPost>(NullLogger<ResPost>.Instance, 3, 0);

            try
            {
                var result = await client.Post($"{server.BaseUrl}/api/res", new ResPost(1));

                Assert.True(result);
                Assert.True(await server.WaitForRequestCountAsync(1));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }

        [Fact]
        public async Task Post_WithResilience_OnServerError_ReturnsFalseAndRetries()
        {
            var server = new TestHttpServer { StatusCode = 500 };
            var client = new RESTClient<ResFail>(NullLogger<ResFail>.Instance, 2, 0);

            try
            {
                var result = await client.Post($"{server.BaseUrl}/api/res", new ResFail(1));

                Assert.False(result);
                Assert.True(await server.WaitForRequestCountAsync(2));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }

        [Fact]
        public async Task Post_WithResilience_RetriesDequeued_WhenRetriesExhausted()
        {
            var server = new TestHttpServer { StatusCode = 500 };
            var client = new RESTClient<ResRetryOff>(NullLogger<ResRetryOff>.Instance, 0, 0);

            try
            {
                var result = await client.Post($"{server.BaseUrl}/api/res", new ResRetryOff(1));

                Assert.False(result);
                Assert.True(await server.WaitForRequestCountAsync(1));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }

        [Fact]
        public async Task Post_WithResilience_RetriesUntilSuccess()
        {
            var server = new TestHttpServer();
            var attempt = 0;
            server.CustomHandler = _ => new TestResponse(++attempt == 1 ? 500 : 200, "{}");
            var client = new RESTClient<ResRetrySuccess>(NullLogger<ResRetrySuccess>.Instance, 3, 0);

            try
            {
                var result = await client.Post($"{server.BaseUrl}/api/res", new ResRetrySuccess(1));

                Assert.False(result);
                Assert.True(await server.WaitForRequestCountAsync(2));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }

        [Fact]
        public async Task Post_WithResilience_AppliesRetriesDelay()
        {
            var server = new TestHttpServer();
            var client = new RESTClient<ResDelay>(NullLogger<ResDelay>.Instance, 3, 50);

            try
            {
                var result = await client.Post($"{server.BaseUrl}/api/res", new ResDelay(1));

                Assert.True(result);
                Assert.True(await server.WaitForRequestCountAsync(1));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }

        [Fact]
        public async Task Post_WithResilience_OnConnectionError_LogsAndQueues()
        {
            var server = new TestHttpServer { CloseWithoutResponding = true };
            var client = new RESTClient<ResError>(NullLogger<ResError>.Instance, 1, 0);

            try
            {
                var result = await client.Post($"{server.BaseUrl}/api/res", new ResError(1));

                Assert.False(result);
                Assert.True(await server.WaitForRequestCountAsync(2));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }

        [Fact]
        public async Task Put_WithResilience_OnSuccess_ReturnsTrue()
        {
            var server = new TestHttpServer();
            var client = new RESTClient<ResPut>(NullLogger<ResPut>.Instance, 2, 0);

            try
            {
                var result = await client.Put($"{server.BaseUrl}/api/res", new ResPut(1));

                Assert.True(result);
                Assert.True(await server.WaitForRequestCountAsync(1));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }

        [Fact]
        public async Task Patch_WithResilience_OnSuccess_ReturnsTrue()
        {
            var server = new TestHttpServer();
            var client = new RESTClient<ResPatch>(NullLogger<ResPatch>.Instance, 2, 0);

            try
            {
                var result = await client.Patch($"{server.BaseUrl}/api/res", new ResPatch(1));

                Assert.True(result);
                Assert.True(await server.WaitForRequestCountAsync(1));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }

        [Fact]
        public async Task Delete_WithResilience_OnSuccess_ReturnsTrue()
        {
            var server = new TestHttpServer();
            var client = new RESTClient<int>(NullLogger<int>.Instance, 2, 0);

            try
            {
                var result = await client.Delete($"{server.BaseUrl}/api/res", "9");

                Assert.True(result);
                Assert.True(await server.WaitForRequestCountAsync(1));
            }
            finally
            {
                client.Dispose();
                server.Dispose();
            }
        }
    }
}