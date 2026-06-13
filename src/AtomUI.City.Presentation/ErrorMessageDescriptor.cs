namespace AtomUI.City.Presentation;

public sealed class ErrorMessageDescriptor
{
    public ErrorMessageDescriptor(
        string errorCode,
        string? message = null,
        string? messageKey = null,
        IReadOnlyList<object?>? messageArguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorCode);

        ErrorCode = errorCode;
        Message = message;
        MessageKey = NormalizeKey(messageKey);
        MessageArguments = messageArguments is null ? null : Array.AsReadOnly(messageArguments.ToArray());
    }

    public string ErrorCode { get; }

    public string? Message { get; }

    public string? MessageKey { get; }

    public IReadOnlyList<object?>? MessageArguments { get; }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key;
    }
}
