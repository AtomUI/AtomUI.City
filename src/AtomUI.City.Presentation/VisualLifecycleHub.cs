using AtomUI.City.Diagnostics;

namespace AtomUI.City.Presentation;

public sealed class VisualLifecycleHub
{
    private readonly object _gate = new();
    private readonly List<Action<VisualLifecycleEvent>> _subscribers = new();
    private readonly IHostDiagnostics? _diagnostics;

    public VisualLifecycleHub()
    {
    }

    public VisualLifecycleHub(IHostDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        _diagnostics = diagnostics;
    }

    public IDisposable Subscribe(Action<VisualLifecycleEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        lock (_gate)
        {
            _subscribers.Add(handler);
        }

        return new Subscription(this, handler);
    }

    public void Notify(object view, VisualLifecycleEventKind kind)
    {
        ArgumentNullException.ThrowIfNull(view);

        Action<VisualLifecycleEvent>[] subscribers;

        lock (_gate)
        {
            subscribers = _subscribers.ToArray();
        }

        var lifecycleEvent = new VisualLifecycleEvent(view, kind);

        foreach (var subscriber in subscribers)
        {
            try
            {
                subscriber(lifecycleEvent);
                WriteAdapterExecutedDiagnostic(lifecycleEvent);
            }
            catch (Exception exception)
            {
                WriteAdapterFailedDiagnostic(lifecycleEvent, exception);

                throw;
            }
        }
    }

    private void Unsubscribe(Action<VisualLifecycleEvent> handler)
    {
        lock (_gate)
        {
            _subscribers.Remove(handler);
        }
    }

    private void WriteAdapterExecutedDiagnostic(VisualLifecycleEvent lifecycleEvent)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.VisualLifecycleAdapterExecuted,
            $"Visual lifecycle adapter handled {lifecycleEvent.Kind} for view '{lifecycleEvent.View.GetType().FullName}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteAdapterFailedDiagnostic(
        VisualLifecycleEvent lifecycleEvent,
        Exception exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.VisualLifecycleAdapterFailed,
            $"Visual lifecycle adapter failed while handling {lifecycleEvent.Kind} for view '{lifecycleEvent.View.GetType().FullName}': {exception.Message}",
            HostDiagnosticSeverity.Error));
    }

    private sealed class Subscription : IDisposable
    {
        private readonly VisualLifecycleHub _hub;
        private Action<VisualLifecycleEvent>? _handler;

        public Subscription(
            VisualLifecycleHub hub,
            Action<VisualLifecycleEvent> handler)
        {
            _hub = hub;
            _handler = handler;
        }

        public void Dispose()
        {
            var handler = _handler;

            if (handler is null)
            {
                return;
            }

            _handler = null;
            _hub.Unsubscribe(handler);
        }
    }
}
