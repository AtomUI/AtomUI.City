namespace AtomUI.City.Testing;

public sealed class PluginTestHost : IDisposable, IAsyncDisposable
{
    private readonly Dictionary<string, PluginTestPackage> _packages;
    private readonly Dictionary<string, PluginTestRecord> _records = new(StringComparer.Ordinal);
    private bool _disposed;

    internal PluginTestHost(TestHost host, IReadOnlyList<PluginTestPackage> packages)
    {
        Host = host;
        _packages = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
    }

    public TestHost Host { get; }

    public IReadOnlyCollection<PluginTestRecord> Records => Array.AsReadOnly(_records.Values.ToArray());

    public static PluginTestHostBuilder CreateBuilder()
    {
        return new PluginTestHostBuilder();
    }

    public ValueTask<PluginTestRecord> InstallAsync(string pluginId)
    {
        var package = GetPackage(pluginId);
        var installPath = Path.Combine(Host.Directory.RootPath, "plugins", "installed", package.Id, package.Version);

        Directory.CreateDirectory(installPath);
        File.WriteAllText(
            Path.Combine(installPath, "plugin.json"),
            $$"""
            {
              "id": "{{package.Id}}",
              "version": "{{package.Version}}"
            }
            """);

        var record = new PluginTestRecord(package.Id, package.Version, installPath, PluginTestState.Installed);
        _records[pluginId] = record;

        return ValueTask.FromResult(record);
    }

    public ValueTask<PluginTestRecord> ActivateAsync(string pluginId)
    {
        var record = GetRecord(pluginId);

        record.State = PluginTestState.Active;

        return ValueTask.FromResult(record);
    }

    public ValueTask<PluginTestRecord> DeactivateAsync(string pluginId)
    {
        var record = GetRecord(pluginId);

        record.State = PluginTestState.Inactive;

        return ValueTask.FromResult(record);
    }

    public ValueTask<PluginTestRecord> UnloadAsync(string pluginId)
    {
        var record = GetRecord(pluginId);

        record.State = PluginTestState.Unloaded;

        return ValueTask.FromResult(record);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Host.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await Host.DisposeAsync().ConfigureAwait(false);
    }

    private PluginTestPackage GetPackage(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        if (!_packages.TryGetValue(pluginId, out var package))
        {
            throw new KeyNotFoundException($"Plugin package '{pluginId}' is not registered.");
        }

        return package;
    }

    private PluginTestRecord GetRecord(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        if (!_records.TryGetValue(pluginId, out var record))
        {
            throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");
        }

        return record;
    }
}
