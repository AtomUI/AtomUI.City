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

        try
        {
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

            TResponse mappedResponse;
            try
            {
                mappedResponse = await httpRequest.ResponseMapper(response).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return DataResult<TResponse>.Failed(
                    new DataError(
                        DataErrorKind.SerializationError,
                        exception.Message,
                        Exception: exception));
            }

            return DataResult<TResponse>.Success(mappedResponse);
        }
        catch (OperationCanceledException)
        {
            return DataResult<TResponse>.Cancelled();
        }
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
