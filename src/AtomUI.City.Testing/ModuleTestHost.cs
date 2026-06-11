using AtomUI.City.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Testing;

public sealed class ModuleTestHost : IDisposable, IAsyncDisposable
{
    private readonly TestHost _host;
    private readonly ServiceCollection _services = [];
    private ServiceProvider? _serviceProvider;
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
            await module.Module.PreConfigureServicesAsync(CreateServiceConfigurationContext()).ConfigureAwait(false);
        }

        foreach (var module in Modules)
        {
            await module.Module.ConfigureServicesAsync(CreateServiceConfigurationContext()).ConfigureAwait(false);
        }

        foreach (var module in Modules)
        {
            await module.Module.PostConfigureServicesAsync(CreateServiceConfigurationContext()).ConfigureAwait(false);
        }

        _serviceProvider = _services.BuildServiceProvider();

        foreach (var module in Modules)
        {
            await module.Module.ConfigureContributionsAsync(CreateContributionConfigurationContext()).ConfigureAwait(false);
        }

        foreach (var module in Modules)
        {
            await module.Module.OnPreApplicationInitializationAsync(CreateApplicationInitializationContext()).ConfigureAwait(false);
        }

        foreach (var module in Modules)
        {
            await module.Module.OnApplicationInitializationAsync(CreateApplicationInitializationContext()).ConfigureAwait(false);
        }

        foreach (var module in Modules)
        {
            await module.Module.OnPostApplicationInitializationAsync(CreateApplicationInitializationContext()).ConfigureAwait(false);
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

        if (_initialized)
        {
            for (var index = Modules.Count - 1; index >= 0; index--)
            {
                await Modules[index].Module.OnApplicationShutdownAsync(CreateApplicationShutdownContext()).ConfigureAwait(false);
            }
        }

        await _host.StopAsync().ConfigureAwait(false);
        await DisposeServiceProviderAsync().ConfigureAwait(false);
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

    private ServiceConfigurationContext CreateServiceConfigurationContext()
    {
        return new ServiceConfigurationContext(_host.ApplicationContext, _services);
    }

    private ContributionConfigurationContext CreateContributionConfigurationContext()
    {
        return new ContributionConfigurationContext(_host.ApplicationContext, GetServiceProvider());
    }

    private ApplicationInitializationContext CreateApplicationInitializationContext()
    {
        return new ApplicationInitializationContext(_host.ApplicationContext, GetServiceProvider());
    }

    private ApplicationShutdownContext CreateApplicationShutdownContext()
    {
        return new ApplicationShutdownContext(_host.ApplicationContext, GetServiceProvider());
    }

    private IServiceProvider GetServiceProvider()
    {
        return _serviceProvider ?? throw new InvalidOperationException("Module test host has not been initialized.");
    }

    private async ValueTask DisposeServiceProviderAsync()
    {
        if (_serviceProvider is null)
        {
            return;
        }

        await _serviceProvider.DisposeAsync().ConfigureAwait(false);
        _serviceProvider = null;
    }
}
