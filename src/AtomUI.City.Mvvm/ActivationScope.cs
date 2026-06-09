namespace AtomUI.City.Mvvm;

public sealed class ActivationScope : IActivationScope
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
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

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _cancellationTokenSource.Cancel();

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }

        _disposables.Clear();
        _cancellationTokenSource.Dispose();
    }
}
