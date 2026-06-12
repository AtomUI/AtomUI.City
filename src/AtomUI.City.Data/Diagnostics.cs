namespace AtomUI.City.Data;

public static class DataDiagnosticIds
{
    public const string RequestRetry = "AUCDATA001";
    public const string ConnectionRegistered = "AUCDATA002";
    public const string ConnectionStopped = "AUCDATA003";
    public const string RequestCompleted = "AUCDATA004";
    public const string RequestFailed = "AUCDATA005";
    public const string CacheReadFailed = "AUCDATA006";
    public const string CacheWriteFailed = "AUCDATA007";
    public const string CacheHit = "AUCDATA008";
    public const string CacheMiss = "AUCDATA009";
    public const string CacheInvalidated = "AUCDATA010";
    public const string ClientMissing = "AUCDATA011";
    public const string ConnectionStopFailed = "AUCDATA012";
    public const string ConnectionStartFailed = "AUCDATA013";
    public const string ConnectionStarted = "AUCDATA014";
}

public sealed record DataDiagnosticRecord(
    string Code,
    string Message,
    DataDiagnosticSeverity Severity,
    Guid? OperationId = null,
    string? ClientId = null,
    string? OperationName = null,
    DataTransportKind? TransportKind = null,
    int? Attempt = null,
    DataErrorKind? ErrorKind = null);

public enum DataDiagnosticSeverity
{
    Trace,
    Info,
    Warning,
    Error,
}

public interface IDataDiagnostics
{
    IReadOnlyList<DataDiagnosticRecord> Records { get; }

    void Write(DataDiagnosticRecord record);
}

public sealed class InMemoryDataDiagnostics : IDataDiagnostics
{
    private readonly List<DataDiagnosticRecord> _records = [];
    private readonly object _syncRoot = new();

    public IReadOnlyList<DataDiagnosticRecord> Records
    {
        get
        {
            lock (_syncRoot)
            {
                return _records.ToArray();
            }
        }
    }

    public void Write(DataDiagnosticRecord record)
    {
        lock (_syncRoot)
        {
            _records.Add(record);
        }
    }
}
