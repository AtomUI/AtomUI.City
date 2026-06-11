namespace AtomUI.City.Mvvm;

public sealed class ActivationScope : IActivationScope
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<IAsyncDisposable> _asyncDisposables = [];
    private readonly List<IDisposable> _disposables = [];
    private bool _isDisposed;

    public ActivationScope()
    {
        CancellationToken = _cancellationTokenSource.Token;
    }

    public CancellationToken CancellationToken { get; }

    public void Add(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);

        if (_isDisposed)
        {
            disposable.Dispose();
            return;
        }

        _disposables.Add(disposable);
    }

    public void AddAsync(IAsyncDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);

        if (_isDisposed)
        {
            disposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
            return;
        }

        _asyncDisposables.Add(disposable);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _cancellationTokenSource.Cancel();

        for (var i = _asyncDisposables.Count - 1; i >= 0; i--)
        {
            _asyncDisposables[i].DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        for (var i = _disposables.Count - 1; i >= 0; i--)
        {
            _disposables[i].Dispose();
        }

        _asyncDisposables.Clear();
        _disposables.Clear();
        _cancellationTokenSource.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);

        for (var i = _asyncDisposables.Count - 1; i >= 0; i--)
        {
            await _asyncDisposables[i].DisposeAsync().ConfigureAwait(false);
        }

        for (var i = _disposables.Count - 1; i >= 0; i--)
        {
            _disposables[i].Dispose();
        }

        _asyncDisposables.Clear();
        _disposables.Clear();
        _cancellationTokenSource.Dispose();
    }
}
