using AtomUI.City.Hosting;

namespace AtomUI.City.Testing;

public sealed class TestHost : IDisposable, IAsyncDisposable
{
    private bool _disposed;

    internal TestHost(
        ApplicationContext applicationContext,
        TestDirectory directory,
        FakeUiDispatcher dispatcher,
        DeterministicScheduler scheduler,
        TestDiagnostics diagnostics)
    {
        ApplicationContext = applicationContext;
        Directory = directory;
        Dispatcher = dispatcher;
        Scheduler = scheduler;
        Diagnostics = diagnostics;
    }

    public ApplicationContext ApplicationContext { get; }

    public TestDirectory Directory { get; }

    public FakeUiDispatcher Dispatcher { get; }

    public DeterministicScheduler Scheduler { get; }

    public TestDiagnostics Diagnostics { get; }

    public bool IsStopped { get; private set; }

    public static TestHostBuilder CreateBuilder()
    {
        return new TestHostBuilder();
    }

    public ValueTask StopAsync()
    {
        IsStopped = true;

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopAsync().AsTask().GetAwaiter().GetResult();
        Directory.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await StopAsync().ConfigureAwait(false);
        Directory.Dispose();
    }
}
