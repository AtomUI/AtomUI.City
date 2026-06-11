namespace AtomUI.City.Data;

public sealed record DataError(
    DataErrorKind Kind,
    string Message,
    string? TransportStatus = null,
    string? MessageKey = null,
    IReadOnlyList<object?>? MessageArguments = null,
    Exception? Exception = null);
