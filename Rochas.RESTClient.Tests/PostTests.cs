using Rochas.Net.Connectivity;

namespace Rochas.RESTClient.Tests
{
    public record Product(int Id, string Description);

    public record ProductResult(bool Created);

    public class PostTests : IDisposable
    {
        private readonly TestHttpServer _server = new();
        private readonly RESTClient<Product> _client = new();

        [Fact]
        public async Task Post_OnSuccess_ReturnsTrue()
        {
            var result = await _client.Post($"{_server.BaseUrl}/api/products", new Product(1, "Laptop"));

            Assert.True(result);
        }

        [Fact]
        public async Task Post_OnServerError_ReturnsFalse()
        {
            _server.StatusCode = 500;

            var result = await _client.Post($"{_server.BaseUrl}/api/products", new Product(1, "Laptop"));

            Assert.False(result);
        }

        [Fact]
        public async Task Post_SendsJsonBody()
        {
            await _client.Post($"{_server.BaseUrl}/api/products", new Product(2, "Mouse"));
            var request = await _server.GetNextRequestAsync();

            Assert.Equal("POST", request.Method);
            Assert.Contains("Mouse", request.Body);
            Assert.Contains("\"Id\":2", request.Body);
        }

        [Fact]
        public async Task Post_WithBlankRoute_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _client.Post("  ", new Product(1, "Laptop")));
        }

        [Fact]
        public async Task PostWithResponse_ReturnsDeserializedResult()
        {
            _server.ResponseBody = """{"created":true}""";

            var result = await _client.PostWithResponse<ProductResult>(
                $"{_server.BaseUrl}/api/products", new Product(1, "Laptop"));

            Assert.NotNull(result);
            Assert.True(result.Created);
        }

        [Fact]
        public async Task PostWithResponse_WithBlankRoute_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _client.PostWithResponse<ProductResult>("", new Product(1, "Laptop")));
        }

        [Fact]
        public void PostSync_OnSuccess_ReturnsTrue()
        {
            var result = _client.PostSync($"{_server.BaseUrl}/api/products", new Product(1, "Laptop"));

            Assert.True(result);
        }

        [Fact]
        public void PostWithResponseSync_ReturnsDeserializedResult()
        {
            _server.ResponseBody = """{"created":false}""";

            var result = _client.PostWithResponseSync<ProductResult>(
                $"{_server.BaseUrl}/api/products", new Product(1, "Laptop"));

            Assert.NotNull(result);
            Assert.False(result.Created);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}