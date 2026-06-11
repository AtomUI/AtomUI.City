namespace AtomUI.City.Data;

public sealed record DataError(
    DataErrorKind Kind,
    string Message,
    string? TransportStatus = null,
    Exception? Exception = null);
