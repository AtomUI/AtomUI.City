namespace AtomUI.City.Lifecycle;

public sealed class LifecycleScope : IDisposable, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly List<LifecycleScope> _children = [];
    private bool _disposed;

    private LifecycleScope(
        LifecycleScopeKind kind,
        string id,
        LifecycleScope? parent,
        CancellationTokenSource cancellationTokenSource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Kind = kind;
        Id = id;
        Parent = parent;
        _cancellationTokenSource = cancellationTokenSource;
        State = LifecycleScopeState.Running;
    }

    public string Id { get; }

    public LifecycleScopeKind Kind { get; }

    public LifecycleScope? Parent { get; }

    public IReadOnlyList<LifecycleScope> Children => _children;

    public LifecycleScopeState State { get; private set; }

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public static LifecycleScope CreateRoot(LifecycleScopeKind kind, string id)
    {
        return new LifecycleScope(kind, id, parent: null, new CancellationTokenSource());
    }

    public LifecycleScope CreateChild(LifecycleScopeKind kind, string id)
    {
        ThrowIfDisposed();

        if (State != LifecycleScopeState.Running)
        {
            throw new InvalidOperationException("Lifecycle scope can only create children while running.");
        }

        var child = new LifecycleScope(
            kind,
            id,
            this,
            CancellationTokenSource.CreateLinkedTokenSource(CancellationToken));

        _children.Add(child);

        return child;
    }

    public async ValueTask StopAsync()
    {
        ThrowIfDisposed();

        if (State is LifecycleScopeState.Stopped or LifecycleScopeState.Stopping)
        {
            return;
        }

        State = LifecycleScopeState.Stopping;

        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        for (var i = _children.Count - 1; i >= 0; i--)
        {
            await _children[i].StopAsync().ConfigureAwait(false);
        }

        State = LifecycleScopeState.Stopped;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopAsync().AsTask().GetAwaiter().GetResult();
        DisposeCoreAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await StopAsync().ConfigureAwait(false);
        await DisposeCoreAsync().ConfigureAwait(false);
    }

    private async ValueTask DisposeCoreAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        State = LifecycleScopeState.Disposing;

        for (var i = _children.Count - 1; i >= 0; i--)
        {
            await _children[i].DisposeAsync().ConfigureAwait(false);
        }

        _cancellationTokenSource.Dispose();
        State = LifecycleScopeState.Disposed;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(LifecycleScope));
        }
    }
}
