using System.Net.Http.Headers;

namespace AtomUI.City.Data;

public sealed class HttpDataTransport : IRequestResponseTransport
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpDataTransport(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public DataTransportKind Kind => DataTransportKind.Http;

    public async ValueTask<DataResult<TResponse>> SendAsync<TResponse>(
        DataRequest<TResponse> request,
        DataRequestContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        if (request is not HttpDataRequest<TResponse> httpRequest)
        {
            return DataResult<TResponse>.Failed(
                new DataError(
                    DataErrorKind.PolicyRejected,
                    "HTTP transport requires an HTTP data request."));
        }

        var client = _httpClientFactory.CreateClient(httpRequest.ClientName);
        using var message = httpRequest.RequestFactory(context);
        AttachCredential(message, context.Credential);

        using var response = await client
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return DataResult<TResponse>.Failed(DataErrorMapper.FromHttpStatusCode(response.StatusCode));
        }

        var mappedResponse = await httpRequest.ResponseMapper(response).ConfigureAwait(false);

        return DataResult<TResponse>.Success(mappedResponse);
    }

    private static void AttachCredential(HttpRequestMessage message, DataCredential? credential)
    {
        if (credential is null)
        {
            return;
        }

        message.Headers.Authorization = new AuthenticationHeaderValue(
            credential.Scheme,
            credential.Parameter);
    }
}
