using Rochas.Net.Connectivity;

namespace Rochas.RESTClient.Tests
{
    public class DeleteTests : IDisposable
    {
        private readonly TestHttpServer _server = new();
        private readonly RESTClient<string> _client = new();

        [Fact]
        public async Task Delete_OnSuccess_ReturnsTrue()
        {
            var result = await _client.Delete($"{_server.BaseUrl}/api/items", "42");

            Assert.True(result);
        }

        [Fact]
        public async Task Delete_OnServerError_ReturnsFalse()
        {
            _server.StatusCode = 500;

            var result = await _client.Delete($"{_server.BaseUrl}/api/items", "42");

            Assert.False(result);
        }

        [Fact]
        public async Task Delete_ComposesRouteWithId()
        {
            await _client.Delete($"{_server.BaseUrl}/api/items", "42");
            var request = await _server.GetNextRequestAsync();

            Assert.Equal("DELETE", request.Method);
            Assert.Equal("/api/items/42", request.Url);
        }

        [Fact]
        public async Task Delete_WithBlankRoute_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _client.Delete(" ", "42"));
        }

        [Fact]
        public async Task DeleteWithParams_EncodesQueryString()
        {
            var parameters = new Dictionary<string, string> { ["force"] = "true" };

            var result = await _client.DeleteWithParams($"{_server.BaseUrl}/api/items", parameters);

            var request = await _server.GetNextRequestAsync();
            Assert.True(result);
            Assert.Equal("DELETE", request.Method);
            Assert.Contains("/api/items?force=true", request.Url);
        }

        [Fact]
        public async Task DeleteWithParams_WithBlankRoute_ThrowsInvalidOperationException()
        {
            var parameters = new Dictionary<string, string> { ["force"] = "true" };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _client.DeleteWithParams("", parameters));
        }

        [Fact]
        public void DeleteSync_OnSuccess_ReturnsTrue()
        {
            var result = _client.DeleteSync($"{_server.BaseUrl}/api/items", "7");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteWithParamsSync_EncodesQueryString()
        {
            var parameters = new Dictionary<string, string> { ["purge"] = "1" };

            var result = _client.DeleteWithParamsSync($"{_server.BaseUrl}/api/items", parameters);

            var request = await _server.GetNextRequestAsync();
            Assert.True(result);
            Assert.Contains("purge=1", request.Url);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}