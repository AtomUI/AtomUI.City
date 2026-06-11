namespace AtomUI.City.Data;

public interface IRequestResponseTransport
{
    DataTransportKind Kind { get; }

    ValueTask<DataResult<TResponse>> SendAsync<TResponse>(
        DataRequest<TResponse> request,
        DataRequestContext context,
        CancellationToken cancellationToken = default);
}
