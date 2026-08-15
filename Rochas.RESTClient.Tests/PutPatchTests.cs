using Rochas.Net.Connectivity;

namespace Rochas.RESTClient.Tests
{
    public record Article(int Id, string Title);

    public record ArticleResult(bool Updated);

    public class PutPatchTests : IDisposable
    {
        private readonly TestHttpServer _server = new();
        private readonly RESTClient<Article> _client = new();

        [Fact]
        public async Task Put_OnSuccess_ReturnsTrue()
        {
            var result = await _client.Put($"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.True(result);
        }

        [Fact]
        public async Task Put_OnServerError_ReturnsFalse()
        {
            _server.StatusCode = 500;

            var result = await _client.Put($"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.False(result);
        }

        [Fact]
        public async Task Put_SendsJsonBody()
        {
            await _client.Put($"{_server.BaseUrl}/api/articles", new Article(3, "Hello"));
            var request = await _server.GetNextRequestAsync();

            Assert.Equal("PUT", request.Method);
            Assert.Contains("Hello", request.Body);
        }

        [Fact]
        public async Task Put_WithBlankRoute_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _client.Put("  ", new Article(1, "Title")));
        }

        [Fact]
        public async Task PutWithResponse_ReturnsDeserializedResult()
        {
            _server.ResponseBody = """{"updated":true}""";

            var result = await _client.PutWithResponse<ArticleResult>(
                $"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.NotNull(result);
            Assert.True(result.Updated);
        }

        [Fact]
        public async Task PutWithResponse_WithBlankRoute_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _client.PutWithResponse<ArticleResult>("", new Article(1, "Title")));
        }

        [Fact]
        public async Task Patch_OnSuccess_ReturnsTrue()
        {
            var result = await _client.Patch($"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.True(result);
        }

        [Fact]
        public async Task Patch_OnServerError_ReturnsFalse()
        {
            _server.StatusCode = 500;

            var result = await _client.Patch($"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.False(result);
        }

        [Fact]
        public async Task Patch_SendsJsonBody()
        {
            await _client.Patch($"{_server.BaseUrl}/api/articles", new Article(4, "Patch Me"));
            var request = await _server.GetNextRequestAsync();

            Assert.Equal("PATCH", request.Method);
            Assert.Contains("Patch Me", request.Body);
        }

        [Fact]
        public async Task Patch_WithBlankRoute_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _client.Patch("  ", new Article(1, "Title")));
        }

        [Fact]
        public async Task PatchWithResponse_ReturnsDeserializedResult()
        {
            _server.ResponseBody = """{"updated":true}""";

            var result = await _client.PatchWithResponse<ArticleResult>(
                $"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.NotNull(result);
            Assert.True(result.Updated);
        }

        [Fact]
        public async Task PatchWithResponse_WithBlankRoute_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _client.PatchWithResponse<ArticleResult>("", new Article(1, "Title")));
        }

        [Fact]
        public void PutSync_OnSuccess_ReturnsTrue()
        {
            var result = _client.PutSync($"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.True(result);
        }

        [Fact]
        public void PutWithResponseSync_ReturnsDeserializedResult()
        {
            _server.ResponseBody = """{"updated":true}""";

            var result = _client.PutWithResponseSync<ArticleResult>(
                $"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.NotNull(result);
            Assert.True(result.Updated);
        }

        [Fact]
        public void PatchSync_OnSuccess_ReturnsTrue()
        {
            var result = _client.PatchSync($"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.True(result);
        }

        [Fact]
        public void PatchWithResponseSync_ReturnsDeserializedResult()
        {
            _server.ResponseBody = """{"updated":false}""";

            var result = _client.PatchWithResponseSync<ArticleResult>(
                $"{_server.BaseUrl}/api/articles", new Article(1, "Title"));

            Assert.NotNull(result);
            Assert.False(result.Updated);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}