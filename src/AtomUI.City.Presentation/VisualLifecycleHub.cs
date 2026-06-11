namespace AtomUI.City.Presentation;

public sealed class VisualLifecycleHub
{
    private readonly object _gate = new();
    private readonly List<Action<VisualLifecycleEvent>> _subscribers = new();

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
            subscriber(lifecycleEvent);
        }
    }

    private void Unsubscribe(Action<VisualLifecycleEvent> handler)
    {
        lock (_gate)
        {
            _subscribers.Remove(handler);
        }
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
