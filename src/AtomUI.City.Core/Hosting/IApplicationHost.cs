using AtomUI.City.Lifecycle;

namespace AtomUI.City.Hosting;

public interface IApplicationHost : IDisposable, IAsyncDisposable
{
    IServiceProvider Services { get; }

    IApplicationContext Context { get; }

    LifecycleScope HostScope { get; }

    LifecycleScope? ApplicationScope { get; }

    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);

    Task RunAsync(CancellationToken cancellationToken = default);
}
