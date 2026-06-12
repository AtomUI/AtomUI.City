using System.Collections.Concurrent;

namespace AtomUI.City.Data;

public sealed class InMemoryDataRequestCache : IDataRequestCache
{
    private readonly ConcurrentDictionary<DataCacheKey, object?> _entries = new();

    public ValueTask<DataCacheLookup<TResponse>> TryGetAsync<TResponse>(
        DataCacheKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();

        if (_entries.TryGetValue(key, out var value) && value is TResponse response)
        {
            return ValueTask.FromResult(DataCacheLookup<TResponse>.Hit(response));
        }

        return ValueTask.FromResult(DataCacheLookup<TResponse>.Miss());
    }

    public ValueTask SetAsync<TResponse>(
        DataCacheKey key,
        TResponse? value,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();

        _entries[key] = value;

        return ValueTask.CompletedTask;
    }

    public ValueTask InvalidateAsync(
        DataCacheKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();

        _entries.TryRemove(key, out _);

        return ValueTask.CompletedTask;
    }
}
