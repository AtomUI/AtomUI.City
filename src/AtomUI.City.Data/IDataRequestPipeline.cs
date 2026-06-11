namespace AtomUI.City.Data;

public interface IDataRequestPipeline
{
    ValueTask<DataResult<TResponse>> SendAsync<TResponse>(
        DataRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
