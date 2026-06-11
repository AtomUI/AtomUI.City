namespace AtomUI.City.Diagnostics;

public sealed class InMemoryHostDiagnostics : IHostDiagnostics
{
    private readonly object _syncRoot = new();
    private readonly List<HostDiagnosticRecord> _records = [];

    public IReadOnlyList<HostDiagnosticRecord> Records
    {
        get
        {
            lock (_syncRoot)
            {
                return _records.ToArray();
            }
        }
    }

    public void Write(HostDiagnosticRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrWhiteSpace(record.Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(record.Message);

        lock (_syncRoot)
        {
            _records.Add(record);
        }
    }
}
