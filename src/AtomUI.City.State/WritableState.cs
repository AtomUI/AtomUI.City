namespace AtomUI.City.State;

public sealed class WritableState<T> : IWritableState<T>
{
    private readonly IEqualityComparer<T> _comparer;
    private readonly List<StateSubscription> _subscriptions = [];
    private readonly object _syncRoot = new();
    private T _value;

    public WritableState(
        T initialValue,
        IEqualityComparer<T>? comparer = null)
    {
        _value = initialValue;
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    public event EventHandler<StateChangedEventArgs<T>>? Changed;

    public T Value
    {
        get
        {
            lock (_syncRoot)
            {
                return _value;
            }
        }
    }

    object? IReadOnlyState.Value => Value;

    public long Version { get; private set; }

    public Type ValueType => typeof(T);

    public void Set(T value)
    {
        SetValue(value);
    }

    public bool SetValue(T value)
    {
        StateChangedEventArgs<T>? args;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            if (_comparer.Equals(_value, value))
            {
                return false;
            }

            var oldValue = _value;
            _value = value;
            Version++;
            args = new StateChangedEventArgs<T>(oldValue, value, Version);
            subscriptions = _subscriptions.ToArray();
        }

        Notify(args, subscriptions);

        return true;
    }

    public bool Update(Func<T, T> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);

        StateChangedEventArgs<T>? args;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            var nextValue = updater(_value);

            if (_comparer.Equals(_value, nextValue))
            {
                return false;
            }

            var oldValue = _value;
            _value = nextValue;
            Version++;
            args = new StateChangedEventArgs<T>(oldValue, nextValue, Version);
            subscriptions = _subscriptions.ToArray();
        }

        Notify(args, subscriptions);

        return true;
    }

    public IStateSubscription OnChange(Action<StateChangedEventArgs<T>> handler)
    {
        return OnChange(handler, StateSubscriptionOptions.Immediate);
    }

    public IStateSubscription OnChange(
        Action<StateChangedEventArgs<T>> handler,
        StateSubscriptionOptions options)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(options);

        var subscription = new StateSubscription(
            args => handler((StateChangedEventArgs<T>)args),
            options);

        lock (_syncRoot)
        {
            _subscriptions.Add(subscription);
        }

        return new RemovingStateSubscription(this, subscription);
    }

    IStateSubscription IReadOnlyState.OnChange(Action<StateChangedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return OnChange(args => handler(args));
    }

    IStateSubscription IReadOnlyState.OnChange(
        Action<StateChangedEventArgs> handler,
        StateSubscriptionOptions options)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return OnChange(args => handler(args), options);
    }

    internal void Restore(T value, long version)
    {
        StateChangedEventArgs<T>? args;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            var oldValue = _value;
            _value = value;
            Version = version;
            args = new StateChangedEventArgs<T>(oldValue, value, Version);
            subscriptions = _subscriptions.ToArray();
        }

        Notify(args, subscriptions);
    }

    private void Notify(
        StateChangedEventArgs<T> args,
        StateSubscription[] subscriptions)
    {
        NotifyChangedEvent(args);

        foreach (var subscription in subscriptions)
        {
            subscription.Notify(args);
        }
    }

    private void NotifyChangedEvent(StateChangedEventArgs<T> args)
    {
        var changed = Changed;

        if (changed is null)
        {
            return;
        }

        foreach (var handler in changed.GetInvocationList().Cast<EventHandler<StateChangedEventArgs<T>>>())
        {
            try
            {
                handler(this, args);
            }
            catch
            {
                // Diagnostics integration is handled by the next State diagnostics phase.
            }
        }
    }

    private void Remove(StateSubscription subscription)
    {
        lock (_syncRoot)
        {
            _subscriptions.Remove(subscription);
        }
    }

    private sealed class RemovingStateSubscription : IStateSubscription
    {
        private readonly WritableState<T> _state;
        private readonly StateSubscription _subscription;
        private bool _disposed;

        public RemovingStateSubscription(
            WritableState<T> state,
            StateSubscription subscription)
        {
            _state = state;
            _subscription = subscription;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _subscription.Dispose();
            _state.Remove(_subscription);
        }
    }
}
