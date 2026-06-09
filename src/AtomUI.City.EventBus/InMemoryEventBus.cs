namespace AtomUI.City.EventBus;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly object _gate = new();
    private readonly Dictionary<Type, List<Subscription>> _subscriptions = [];

    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        var subscription = new Subscription(this, eventType, (eventData, cancellationToken) => handler((TEvent)eventData, cancellationToken));

        lock (_gate)
        {
            if (!_subscriptions.TryGetValue(eventType, out var subscriptions))
            {
                subscriptions = [];
                _subscriptions[eventType] = subscriptions;
            }

            subscriptions.Add(subscription);
        }

        return subscription;
    }

    public async ValueTask PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
    {
        Subscription[] subscriptions;
        var eventType = typeof(TEvent);

        lock (_gate)
        {
            subscriptions = _subscriptions.TryGetValue(eventType, out var registeredSubscriptions)
                ? [.. registeredSubscriptions]
                : [];
        }

        foreach (var subscription in subscriptions)
        {
            await subscription.HandleAsync(eventData!, cancellationToken).ConfigureAwait(false);
        }
    }

    private void Unsubscribe(Subscription subscription)
    {
        lock (_gate)
        {
            if (!_subscriptions.TryGetValue(subscription.EventType, out var subscriptions))
            {
                return;
            }

            subscriptions.Remove(subscription);
            if (subscriptions.Count == 0)
            {
                _subscriptions.Remove(subscription.EventType);
            }
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly InMemoryEventBus _eventBus;
        private readonly Func<object, CancellationToken, ValueTask> _handler;
        private bool _isDisposed;

        public Subscription(InMemoryEventBus eventBus, Type eventType, Func<object, CancellationToken, ValueTask> handler)
        {
            _eventBus = eventBus;
            EventType = eventType;
            _handler = handler;
        }

        public Type EventType { get; }

        public ValueTask HandleAsync(object eventData, CancellationToken cancellationToken)
        {
            return _isDisposed ? ValueTask.CompletedTask : _handler(eventData, cancellationToken);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _eventBus.Unsubscribe(this);
        }
    }
}
