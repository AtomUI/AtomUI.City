using AtomUI.City.Modularity;

namespace AtomUI.City.Testing;

public sealed class ModuleTestHost : IDisposable, IAsyncDisposable
{
    private readonly TestHost _host;
    private bool _disposed;
    private bool _initialized;
    private bool _shutdown;

    internal ModuleTestHost(TestHost host, IReadOnlyList<ModuleTestRecord> modules)
    {
        _host = host;
        Modules = modules;
    }

    public IReadOnlyList<ModuleTestRecord> Modules { get; }

    public TestHost Host => _host;

    public static ModuleTestHostBuilder CreateBuilder()
    {
        return new ModuleTestHostBuilder();
    }

    public async ValueTask InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        foreach (var module in Modules)
        {
            await module.Module.PreConfigureAsync(CreateContext(module)).ConfigureAwait(false);
        }

        foreach (var module in Modules)
        {
            await module.Module.ConfigureAsync(CreateContext(module)).ConfigureAwait(false);
        }

        foreach (var module in Modules)
        {
            await module.Module.InitializeAsync(CreateContext(module)).ConfigureAwait(false);
        }

        _initialized = true;
    }

    public async ValueTask ShutdownAsync()
    {
        if (_shutdown)
        {
            return;
        }

        _shutdown = true;

        for (var index = Modules.Count - 1; index >= 0; index--)
        {
            await Modules[index].Module.ShutdownAsync(CreateContext(Modules[index])).ConfigureAwait(false);
        }

        await _host.StopAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        ShutdownAsync().AsTask().GetAwaiter().GetResult();
        _host.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await ShutdownAsync().ConfigureAwait(false);
        await _host.DisposeAsync().ConfigureAwait(false);
    }

    private ModuleContext CreateContext(ModuleTestRecord module)
    {
        return new ModuleContext(module.Name, _host.ApplicationContext);
    }
}
