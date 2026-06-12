using System.Collections.Concurrent;

namespace AtomUI.City.Data;

public sealed class InMemoryDataRequestCache : IDataRequestCache
{
    private readonly ConcurrentDictionary<DataCacheKey, CacheEntry> _entries = new();
    private readonly IDataDiagnostics? _diagnostics;

    public InMemoryDataRequestCache(IDataDiagnostics? diagnostics = null)
    {
        _diagnostics = diagnostics;
    }

    public ValueTask<DataCacheLookup<TResponse>> TryGetAsync<TResponse>(
        DataCacheKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();

        if (_entries.TryGetValue(key, out var entry) && entry.ResponseType == typeof(TResponse))
        {
            return ValueTask.FromResult(DataCacheLookup<TResponse>.Hit((TResponse?)entry.Value));
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

        _entries[key] = new CacheEntry(typeof(TResponse), value);

        return ValueTask.CompletedTask;
    }

    public ValueTask InvalidateAsync(
        DataCacheKey key,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        cancellationToken.ThrowIfCancellationRequested();

        _entries.TryRemove(key, out _);
        _diagnostics?.Write(new DataDiagnosticRecord(
            DataDiagnosticIds.CacheInvalidated,
            $"Data operation '{key.OperationName}' cache entry invalidated.",
            DataDiagnosticSeverity.Info,
            ClientId: key.ClientId,
            OperationName: key.OperationName,
            TransportKind: key.TransportKind));

        return ValueTask.CompletedTask;
    }

    private sealed record CacheEntry(Type ResponseType, object? Value);
}
