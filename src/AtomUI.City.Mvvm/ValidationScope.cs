namespace AtomUI.City.Mvvm;

public sealed class ValidationScope
{
    private readonly Dictionary<string, IReadOnlyList<string>> _errors = new(StringComparer.Ordinal);

    public ValidationStatus Status { get; private set; } = ValidationStatus.Valid;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors => _errors;

    public Exception? Exception { get; private set; }

    public void BindTo(IActivationScope activationScope)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        activationScope.Add(new DelegateDisposable(Cancel));
    }

    public void SetInvalid(string key, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Exception = null;
        _errors[key] = [message];
        Status = ValidationStatus.Invalid;
    }

    public void SetPending()
    {
        Exception = null;
        Status = ValidationStatus.Pending;
    }

    public void SetFailed(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        _errors.Clear();
        Exception = exception;
        Status = ValidationStatus.Failed;
    }

    public void Cancel()
    {
        Exception = null;
        Status = ValidationStatus.Canceled;
    }

    private sealed class DelegateDisposable : IDisposable
    {
        private readonly Action _dispose;
        private bool _disposed;

        public DelegateDisposable(Action dispose)
        {
            _dispose = dispose;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _dispose();
        }
    }
}
