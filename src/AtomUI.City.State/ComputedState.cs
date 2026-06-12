using AtomUI.City.Diagnostics;

namespace AtomUI.City.State;

public sealed class ComputedState<T> : IComputedState<T>, IDisposable
{
    private readonly object _syncRoot = new();
    private readonly Func<T> _compute;
    private readonly IHostDiagnostics? _diagnostics;
    private readonly List<StateSubscription> _subscriptions = [];
    private readonly List<IStateSubscription> _dependencySubscriptions = [];
    private bool _hasValue;
    private bool _isDisposed;
    private T? _value;

    public ComputedState(Func<T> compute, params IReadOnlyState[] dependencies)
        : this(compute, diagnostics: null, dependencies)
    {
    }

    public ComputedState(
        Func<T> compute,
        IHostDiagnostics? diagnostics,
        params IReadOnlyState[] dependencies)
    {
        ArgumentNullException.ThrowIfNull(compute);
        ArgumentNullException.ThrowIfNull(dependencies);

        _compute = compute;
        _diagnostics = diagnostics;

        foreach (var dependency in dependencies)
        {
            _dependencySubscriptions.Add(dependency.OnChange(_ => RecomputeAndNotify()));
        }
    }

    public T Value
    {
        get
        {
            lock (_syncRoot)
            {
                ThrowIfDisposed();
                EnsureValue();

                return _value!;
            }
        }
    }

    object? IReadOnlyState.Value => Value;

    public long Version { get; private set; }

    public Type ValueType => typeof(T);

    public Exception? LastError { get; private set; }

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

        lock (_syncRoot)
        {
            ThrowIfDisposed();
            EnsureValue();

            var subscription = new StateSubscription(
                args => handler((StateChangedEventArgs<T>)args),
                options,
                _diagnostics);

            _subscriptions.Add(subscription);

            return subscription;
        }
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

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        foreach (var subscription in _dependencySubscriptions)
        {
            subscription.Dispose();
        }

        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }

        _dependencySubscriptions.Clear();
        _subscriptions.Clear();
    }

    private void RecomputeAndNotify()
    {
        StateChangedEventArgs<T>? change = null;
        StateSubscription[] subscriptions;

        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return;
            }

            var oldValue = _value;
            var hadValue = _hasValue;
            var newValue = TryCompute();

            if (!_hasValue)
            {
                return;
            }

            if (hadValue && EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                return;
            }

            Version++;
            change = new StateChangedEventArgs<T>(oldValue!, newValue!, Version);
            subscriptions = _subscriptions.ToArray();
        }

        foreach (var subscription in subscriptions)
        {
            subscription.Notify(change);
        }
    }

    private void EnsureValue()
    {
        if (_hasValue)
        {
            return;
        }

        TryCompute();
    }

    private T? TryCompute()
    {
        try
        {
            _value = _compute();
            _hasValue = true;
            LastError = null;

            return _value;
        }
        catch (Exception exception)
        {
            LastError = exception;
            _diagnostics?.Write(new HostDiagnosticRecord(
                StateDiagnosticIds.ComputedStateComputeFailed,
                $"Computed state failed to compute value type '{typeof(T).FullName}': {exception.Message}",
                HostDiagnosticSeverity.Error));

            return _value;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(ComputedState<T>));
        }
    }
}
