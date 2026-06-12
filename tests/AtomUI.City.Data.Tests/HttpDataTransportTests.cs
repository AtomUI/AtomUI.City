using System.Net;
using AtomUI.City.Data;

namespace AtomUI.City.Data.Tests;

public sealed class HttpDataTransportTests
{
    [Fact]
    public async Task HttpTransportSendsRequestThroughNamedHttpClient()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("payload"),
            });
        var transport = new HttpDataTransport(new RecordingHttpClientFactory("api", handler));
        var request = new HttpDataRequest<string>(
            "catalog",
            "get-items",
            "api",
            _ => new HttpRequestMessage(HttpMethod.Get, "https://server/items"),
            async response => await response.Content.ReadAsStringAsync());

        var result = await transport.SendAsync(
            request,
            DataRequestContext.Create(request, CancellationToken.None));

        Assert.True(result.Succeeded);
        Assert.Equal("payload", result.Value);
        Assert.Equal("api", handler.ClientName);
        Assert.Equal(HttpMethod.Get, handler.Requests.Single().Method);
    }

    [Fact]
    public async Task HttpTransportAttachesBearerCredential()
    {
        var handler = new RecordingHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("authorized"),
            });
        var transport = new HttpDataTransport(new RecordingHttpClientFactory("api", handler));
        var request = new HttpDataRequest<string>(
            "catalog",
            "get-items",
            "api",
            _ => new HttpRequestMessage(HttpMethod.Get, "https://server/items"),
            async response => await response.Content.ReadAsStringAsync());
        var context = DataRequestContext.Create(request, CancellationToken.None);
        context.SetCredential(DataCredential.Bearer("access-token"));

        var result = await transport.SendAsync(request, context);

        Assert.True(result.Succeeded);
        Assert.Equal("Bearer", handler.Requests.Single().Headers.Authorization?.Scheme);
        Assert.Equal("access-token", handler.Requests.Single().Headers.Authorization?.Parameter);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, DataErrorKind.AuthenticationRequired)]
    [InlineData(HttpStatusCode.Forbidden, DataErrorKind.AuthorizationForbidden)]
    [InlineData(HttpStatusCode.NotFound, DataErrorKind.NotFound)]
    [InlineData(HttpStatusCode.Conflict, DataErrorKind.Conflict)]
    [InlineData(HttpStatusCode.TooManyRequests, DataErrorKind.PolicyRejected)]
    [InlineData(HttpStatusCode.InternalServerError, DataErrorKind.ServerError)]
    public async Task HttpTransportMapsStatusCodes(HttpStatusCode statusCode, DataErrorKind expectedError)
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(statusCode));
        var transport = new HttpDataTransport(new RecordingHttpClientFactory("api", handler));
        var request = new HttpDataRequest<string>(
            "catalog",
            "get-items",
            "api",
            _ => new HttpRequestMessage(HttpMethod.Get, "https://server/items"),
            _ => ValueTask.FromResult("unused"));

        var result = await transport.SendAsync(
            request,
            DataRequestContext.Create(request, CancellationToken.None));

        Assert.False(result.Succeeded);
        Assert.Equal(expectedError, result.Error?.Kind);
    }

    private sealed class RecordingHttpClientFactory : IHttpClientFactory
    {
        private readonly string _clientName;
        private readonly RecordingHttpMessageHandler _handler;

        public RecordingHttpClientFactory(string clientName, RecordingHttpMessageHandler handler)
        {
            _clientName = clientName;
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            _handler.ClientName = name;

            return name == _clientName
                ? new HttpClient(_handler, disposeHandler: false)
                : throw new InvalidOperationException($"Unexpected client '{name}'.");
        }
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        public string? ClientName { get; set; }

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(_handler(request));
        }
    }
}
