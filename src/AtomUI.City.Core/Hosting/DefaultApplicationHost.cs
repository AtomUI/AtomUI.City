using AtomUI.City.Diagnostics;
using AtomUI.City.Lifecycle;
using AtomUI.City.Modularity;
using Microsoft.Extensions.Hosting;

namespace AtomUI.City.Hosting;

internal sealed class DefaultApplicationHost : IApplicationHost
{
    private readonly IHost _genericHost;
    private readonly IHostDiagnostics _diagnostics;
    private readonly IModuleRegistry _moduleRegistry;
    private readonly SemaphoreSlim _stateLock = new(1, 1);
    private bool _disposed;
    private bool _started;
    private bool _stopped;

    public DefaultApplicationHost(
        IHost genericHost,
        ApplicationContext context,
        IHostDiagnostics diagnostics,
        LifecycleScope hostScope,
        IModuleRegistry moduleRegistry)
    {
        ArgumentNullException.ThrowIfNull(genericHost);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(hostScope);
        ArgumentNullException.ThrowIfNull(moduleRegistry);

        _genericHost = genericHost;
        _diagnostics = diagnostics;
        _moduleRegistry = moduleRegistry;
        Context = context;
        HostScope = hostScope;
    }

    public IServiceProvider Services => _genericHost.Services;

    public IApplicationContext Context { get; }

    public LifecycleScope HostScope { get; }

    public LifecycleScope? ApplicationScope { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _stateLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_started)
            {
                return;
            }

            if (_stopped)
            {
                throw new InvalidOperationException("Application host cannot be started after it has stopped.");
            }

            await _genericHost.StartAsync(cancellationToken).ConfigureAwait(false);
            ApplicationScope = HostScope.CreateChild(LifecycleScopeKind.Application, "application");
            await _moduleRegistry.ConfigureContributionsAsync(
                (ApplicationContext)Context,
                Services,
                cancellationToken).ConfigureAwait(false);
            await _moduleRegistry.InitializeAsync(
                (ApplicationContext)Context,
                Services,
                cancellationToken).ConfigureAwait(false);
            _started = true;
            _diagnostics.Write(new HostDiagnosticRecord(
                HostDiagnosticIds.HostStarted,
                "Application host has started.",
                HostDiagnosticSeverity.Info));
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _stateLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (!_started || _stopped)
            {
                return;
            }

            await _moduleRegistry.ShutdownAsync(
                (ApplicationContext)Context,
                Services,
                cancellationToken).ConfigureAwait(false);

            if (ApplicationScope is not null)
            {
                await ApplicationScope.StopAsync().ConfigureAwait(false);
            }

            await HostScope.StopAsync().ConfigureAwait(false);
            await _genericHost.StopAsync(cancellationToken).ConfigureAwait(false);
            _stopped = true;
            _diagnostics.Write(new HostDiagnosticRecord(
                HostDiagnosticIds.HostStopped,
                "Application host has stopped.",
                HostDiagnosticSeverity.Info));
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await StartAsync(cancellationToken).ConfigureAwait(false);
        await _genericHost.WaitForShutdownAsync(cancellationToken).ConfigureAwait(false);
        await StopAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopAsync().GetAwaiter().GetResult();
        _disposed = true;
        HostScope.Dispose();
        _genericHost.Dispose();
        _stateLock.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await StopAsync().ConfigureAwait(false);
        _disposed = true;
        await HostScope.DisposeAsync().ConfigureAwait(false);

        if (_genericHost is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            _stateLock.Dispose();
            return;
        }

        _genericHost.Dispose();
        _stateLock.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DefaultApplicationHost));
        }
    }
}
