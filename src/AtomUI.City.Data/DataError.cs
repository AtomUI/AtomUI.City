namespace AtomUI.City.Data;

public sealed record DataError(
    DataErrorKind Kind,
    string Message,
    string? TransportStatus = null,
    string? MessageKey = null,
    IReadOnlyList<object?>? MessageArguments = null,
    Exception? Exception = null)
{
    private readonly IReadOnlyList<object?>? _messageArguments =
        MessageArguments is null ? null : Array.AsReadOnly(MessageArguments.ToArray());

    public IReadOnlyList<object?>? MessageArguments
    {
        get => _messageArguments;
        init => _messageArguments = value is null ? null : Array.AsReadOnly(value.ToArray());
    }
}
