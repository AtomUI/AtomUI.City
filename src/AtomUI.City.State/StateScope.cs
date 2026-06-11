namespace AtomUI.City.State;

public sealed class StateScope : IStateScope
{
    private readonly List<IDisposable> _subscriptions = [];
    private bool _disposed;

    public StateScope(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
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
            _subscriptions[i].Dispose();
        }

        _subscriptions.Clear();
        State = StateScopeState.Disposed;
    }
}
