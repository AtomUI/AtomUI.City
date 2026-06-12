namespace AtomUI.City.Data;

public interface IDataRequestCache
{
    ValueTask<DataCacheLookup<TResponse>> TryGetAsync<TResponse>(
        DataCacheKey key,
        CancellationToken cancellationToken = default);

    ValueTask SetAsync<TResponse>(
        DataCacheKey key,
        TResponse? value,
        CancellationToken cancellationToken = default);

    ValueTask InvalidateAsync(
        DataCacheKey key,
        CancellationToken cancellationToken = default);
}
