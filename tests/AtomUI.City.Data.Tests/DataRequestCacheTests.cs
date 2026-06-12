namespace AtomUI.City.Data.Tests;

public sealed class DataRequestCacheTests
{
    [Fact]
    public async Task InMemoryCacheStoresAndReturnsValuesByCacheKey()
    {
        var cache = new InMemoryDataRequestCache();
        var key = CreateKey("items:v1");

        var miss = await cache.TryGetAsync<string>(key);
        await cache.SetAsync(key, "cached");
        var hit = await cache.TryGetAsync<string>(key);

        Assert.False(miss.IsHit);
        Assert.True(hit.IsHit);
        Assert.Equal("cached", hit.Value);
    }

    [Fact]
    public async Task InMemoryCacheStoresNullReferenceValuesByCacheKey()
    {
        var cache = new InMemoryDataRequestCache();
        var key = CreateKey("nullable:v1");

        await cache.SetAsync<string?>(key, null);
        var lookup = await cache.TryGetAsync<string?>(key);

        Assert.True(lookup.IsHit);
        Assert.Null(lookup.Value);
    }

    [Fact]
    public async Task InMemoryCacheInvalidatesEntryByCacheKey()
    {
        var cache = new InMemoryDataRequestCache();
        var key = CreateKey("items:v1");

        await cache.SetAsync(key, "cached");
        await cache.InvalidateAsync(key);
        var lookup = await cache.TryGetAsync<string>(key);

        Assert.False(lookup.IsHit);
    }

    [Fact]
    public async Task InMemoryCacheWritesInvalidationDiagnostic()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        var cache = new InMemoryDataRequestCache(diagnostics);
        var key = CreateKey("items:v1");

        await cache.InvalidateAsync(key);

        var record = Assert.Single(
            diagnostics.Records,
            record => record.Code == DataDiagnosticIds.CacheInvalidated);
        Assert.Equal("catalog", record.ClientId);
        Assert.Equal("get-items", record.OperationName);
        Assert.Equal(DataTransportKind.Http, record.TransportKind);
    }

    private static DataCacheKey CreateKey(string requestFingerprint)
    {
        return new DataCacheKey(
            "catalog",
            "get-items",
            DataTransportKind.Http,
            DataAccessMode.Query,
            requestFingerprint,
            "Anonymous",
            "anonymous",
            "default",
            PluginContributionId: null,
            "default",
            "default");
    }
}
