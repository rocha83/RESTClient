using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Rochas.RESTClient.Tests
{
    public sealed record TestRequest(string Method, string Url, string Body, IDictionary<string, string> Headers)
    {
        public string Header(string name) =>
            Headers.TryGetValue(name, out var value) ? value : string.Empty;
    }

    public sealed record TestResponse(int Status, string Body);

    public sealed class TestHttpServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _acceptLoop;

        public readonly ConcurrentQueue<TestRequest> Requests = new();

        public int StatusCode { get; set; } = 200;
        public string ResponseBody { get; set; } = "{}";
        public bool CloseWithoutResponding { get; set; }
        public Func<TestRequest, TestResponse>? CustomHandler { get; set; }

        public int RequestCount => Requests.Count;
        public string BaseUrl { get; }

        public TestHttpServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            BaseUrl = $"http://127.0.0.1:{port}";
            _acceptLoop = Task.Run(() => AcceptLoopAsync(_cts.Token));
        }

        public async Task<TestRequest> GetNextRequestAsync(int timeoutMilliseconds = 10000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                if (Requests.TryDequeue(out var request))
                    return request;
                await Task.Delay(25);
            }

            throw new TimeoutException("No request was received by the test server.");
        }

        public async Task<bool> WaitForRequestCountAsync(int count, int timeoutMilliseconds = 10000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < deadline)
            {
                if (RequestCount >= count)
                    return true;
                await Task.Delay(25);
            }

            return false;
        }

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(token);
                }
                catch
                {
                    break;
                }

                _ = Task.Run(() => HandleClientAsync(client));
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8, false, 8192, true))
                {
                    var requestLine = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(requestLine))
                        return;

                    var parts = requestLine.Split(' ');
                    var method = parts.Length > 0 ? parts[0] : string.Empty;
                    var url = parts.Length > 1 ? parts[1] : "/";

                    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    var contentLength = 0;
                    string? line;
                    while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
                    {
                        var separator = line.IndexOf(':');
                        if (separator <= 0)
                            continue;

                        var name = line.Substring(0, separator).Trim();
                        var value = line.Substring(separator + 1).Trim();
                        headers[name] = value;
                        if (name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                            contentLength = int.Parse(value);
                    }

                    var body = string.Empty;
                    if (contentLength > 0)
                    {
                        var buffer = new char[contentLength];
                        var read = 0;
                        while (read < contentLength)
                        {
                            var chunk = await reader.ReadAsync(buffer, read, contentLength - read);
                            if (chunk == 0)
                                break;
                            read += chunk;
                        }

                        body = new string(buffer, 0, read);
                    }

                    System.Console.Error.WriteLine($"REQ {method} {url} CL={contentLength} TE={headers.GetValueOrDefault("Transfer-Encoding")} BODYLEN={body.Length}");

                    if (CloseWithoutResponding)
                        return;

                    var response = CustomHandler != null
                        ? CustomHandler(new TestRequest(method, url, body, headers))
                        : new TestResponse(StatusCode, ResponseBody);

                    var payload = Encoding.UTF8.GetBytes(response.Body ?? string.Empty);
                    var head = Encoding.UTF8.GetBytes(
                        $"HTTP/1.1 {response.Status} OK\r\nContent-Type: application/json\r\nContent-Length: {payload.Length}\r\nConnection: close\r\n\r\n");
                    var message = new byte[head.Length + payload.Length];
                    Buffer.BlockCopy(head, 0, message, 0, head.Length);
                    Buffer.BlockCopy(payload, 0, message, head.Length, payload.Length);
                    await stream.WriteAsync(message, 0, message.Length);
                }
            }
            catch (Exception)
            {
                // Swallow client/IO errors: the test scenario decides the outcome.
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            try
            {
                _listener.Stop();
            }
            catch (Exception)
            {
                // Listener may already be stopped.
            }
        }
    }
}