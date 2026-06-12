using AtomUI.City.Diagnostics;

namespace AtomUI.City.State;

public sealed class StateScope : IStateScope
{
    private readonly IHostDiagnostics? _diagnostics;
    private readonly List<IDisposable> _subscriptions = [];
    private bool _disposed;

    public StateScope(string id, IHostDiagnostics? diagnostics = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        _diagnostics = diagnostics;
    }

    public string Id { get; }

    public StateScopeState State { get; private set; } = StateScopeState.Active;

    public void Add(IDisposable subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        if (_disposed)
        {
            subscription.Dispose();
            return;
        }

        _subscriptions.Add(subscription);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        State = StateScopeState.Disposing;

        for (var i = _subscriptions.Count - 1; i >= 0; i--)
        {
            try
            {
                _subscriptions[i].Dispose();
            }
            catch (Exception exception)
            {
                WriteDisposeFailedDiagnostic(exception);
            }
        }

        _subscriptions.Clear();
        State = StateScopeState.Disposed;
    }

    private void WriteDisposeFailedDiagnostic(Exception exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            StateDiagnosticIds.StateScopeDisposeFailed,
            $"State scope '{Id}' subscription disposal failed: {exception.Message}",
            HostDiagnosticSeverity.Error));
    }
}
