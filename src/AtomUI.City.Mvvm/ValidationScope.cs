using System.Collections.ObjectModel;

namespace AtomUI.City.Mvvm;

public sealed class ValidationScope
{
    private readonly Dictionary<string, IReadOnlyList<string>> _errors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IReadOnlyList<ValidationMessage>> _messages = new(StringComparer.Ordinal);
    private readonly ReadOnlyDictionary<string, IReadOnlyList<string>> _readOnlyErrors;
    private readonly ReadOnlyDictionary<string, IReadOnlyList<ValidationMessage>> _readOnlyMessages;

    public ValidationScope()
    {
        _readOnlyErrors = new ReadOnlyDictionary<string, IReadOnlyList<string>>(_errors);
        _readOnlyMessages = new ReadOnlyDictionary<string, IReadOnlyList<ValidationMessage>>(_messages);
    }

    public ValidationStatus Status { get; private set; } = ValidationStatus.Valid;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors => _readOnlyErrors;

    public IReadOnlyDictionary<string, IReadOnlyList<ValidationMessage>> Messages => _readOnlyMessages;

    public Exception? Exception { get; private set; }

    public void BindTo(IActivationScope activationScope)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        activationScope.Add(new DelegateDisposable(Cancel));
    }

    public void SetInvalid(
        string key,
        string message,
        string? messageKey = null,
        IReadOnlyList<object?>? messageArguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Exception = null;
        _errors[key] = Array.AsReadOnly(new[] { message });
        _messages[key] = Array.AsReadOnly(new[]
        {
            new ValidationMessage(
                key,
                message,
                messageKey,
                messageArguments),
        });
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
        _messages.Clear();
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
