namespace AtomUI.City.Mvvm;

public sealed class ValidationMessage
{
    public ValidationMessage(
        string key,
        string message,
        string? messageKey = null,
        IReadOnlyList<object?>? messageArguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        Key = key;
        Message = message;
        MessageKey = messageKey;
        MessageArguments = messageArguments is null ? null : Array.AsReadOnly(messageArguments.ToArray());
    }

    public string Key { get; }

    public string Message { get; }

    public string? MessageKey { get; }

    public IReadOnlyList<object?>? MessageArguments { get; }
}
