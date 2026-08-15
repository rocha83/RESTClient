using Rochas.Net.Connectivity;

namespace Rochas.RESTClient.Tests
{
    public record Customer(int Id, string Name);

    public class GetTests : IDisposable
    {
        private readonly TestHttpServer _server = new();
        private readonly RESTClient<Customer> _client = new();

        [Fact]
        public async Task Get_ReturnsDeserializedPayload()
        {
            _server.ResponseBody = """{"id":7,"name":"Ada Lovelace"}""";

            var result = await _client.Get($"{_server.BaseUrl}/api/customers");

            Assert.NotNull(result);
            Assert.Equal(7, result.Id);
            Assert.Equal("Ada Lovelace", result.Name);
        }

        [Fact]
        public async Task Get_WithHeaders_SendsHeaders()
        {
            var headers = new Dictionary<string, string> { ["X-Token"] = "abc123", ["Accept-Language"] = "pt-BR" };

            await _client.Get($"{_server.BaseUrl}/api/customers", headers);
            var request = await _server.GetNextRequestAsync();

            Assert.Equal("abc123", request.Header("X-Token"));
            Assert.Equal("pt-BR", request.Header("Accept-Language"));
        }

        [Fact]
        public async Task Get_WithPositiveTimeout_Completes()
        {
            _server.ResponseBody = """{"id":1,"name":"Grace Hopper"}""";

            var result = await _client.Get($"{_server.BaseUrl}/api/customers", null, 30);

            Assert.NotNull(result);
            Assert.Equal("Grace Hopper", result.Name);
        }

        [Fact]
        public async Task Get_WithBlankRoute_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _client.Get("   "));
        }

        [Fact]
        public async Task Get_WithId_ComposesRouteWithId()
        {
            _server.ResponseBody = """{"id":42,"name":"Alan Turing"}""";

            var result = await _client.Get($"{_server.BaseUrl}/api/customers", "42");

            var request = await _server.GetNextRequestAsync();
            Assert.Equal("GET", request.Method);
            Assert.Equal("/api/customers/42", request.Url);
            Assert.Equal(42, result.Id);
        }

        [Fact]
        public async Task Get_WithIdAndBlankRoute_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _client.Get("  ", "42"));
        }

        [Fact]
        public async Task GetWithParams_EncodesQueryString()
        {
            var parameters = new Dictionary<string, string> { ["name"] = "John Doe", ["age"] = "30" };
            _server.ResponseBody = """{"id":5,"name":"John Doe"}""";

            var result = await _client.GetWithParams($"{_server.BaseUrl}/api/search", parameters);

            var request = await _server.GetNextRequestAsync();
            Assert.StartsWith("/api/search?", request.Url);
            Assert.Contains("name=John%20Doe", request.Url);
            Assert.Contains("age=30", request.Url);
            Assert.Equal(5, result.Id);
        }

        [Fact]
        public async Task GetWithParams_WithBlankRoute_ThrowsArgumentException()
        {
            var parameters = new Dictionary<string, string> { ["name"] = "John Doe" };

            await Assert.ThrowsAsync<ArgumentException>(() => _client.GetWithParams(" ", parameters));
        }

        [Fact]
        public void GetSync_ReturnsDeserializedPayload()
        {
            _server.ResponseBody = """{"id":3,"name":"Katherine Johnson"}""";

            var result = _client.GetSync($"{_server.BaseUrl}/api/customers");

            Assert.NotNull(result);
            Assert.Equal(3, result.Id);
            Assert.Equal("Katherine Johnson", result.Name);
        }

        [Fact]
        public async Task GetSync_WithId_ComposesRouteWithId()
        {
            _server.ResponseBody = """{"id":9,"name":"Dijkstra"}""";

            var result = _client.GetSync($"{_server.BaseUrl}/api/customers", "9");

            var request = await _server.GetNextRequestAsync();
            Assert.Equal("/api/customers/9", request.Url);
            Assert.Equal(9, result.Id);
        }

        [Fact]
        public async Task GetWithParamsSync_EncodesQueryString()
        {
            var parameters = new Dictionary<string, string> { ["filter"] = "a&b" };
            _server.ResponseBody = """{"id":1,"name":"Linus"}""";

            var result = _client.GetWithParamsSync($"{_server.BaseUrl}/api/search", parameters);

            var request = await _server.GetNextRequestAsync();
            Assert.Contains("filter=a%26b", request.Url);
            Assert.NotNull(result);
        }

        public void Dispose()
        {
            _client.Dispose();
            _server.Dispose();
        }
    }
}