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
    public async Task InMemoryCacheInvalidatesEntryByCacheKey()
    {
        var cache = new InMemoryDataRequestCache();
        var key = CreateKey("items:v1");

        await cache.SetAsync(key, "cached");
        await cache.InvalidateAsync(key);
        var lookup = await cache.TryGetAsync<string>(key);

        Assert.False(lookup.IsHit);
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
