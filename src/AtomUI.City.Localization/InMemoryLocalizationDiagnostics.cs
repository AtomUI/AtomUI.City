namespace AtomUI.City.Localization;

public sealed class InMemoryLocalizationDiagnostics : ILocalizationDiagnostics
{
    private readonly List<LocalizationDiagnosticRecord> _records = [];
    private readonly object _syncRoot = new();

    public IReadOnlyList<LocalizationDiagnosticRecord> Records
    {
        get
        {
            lock (_syncRoot)
            {
                return Array.AsReadOnly(_records.ToArray());
            }
        }
    }

    public void Write(LocalizationDiagnosticRecord record)
    {
        lock (_syncRoot)
        {
            _records.Add(record);
        }
    }
}
