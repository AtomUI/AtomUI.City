namespace AtomUI.City.Testing;

public sealed class TestDiagnostics
{
    private readonly List<TestDiagnosticEntry> _entries = [];

    public IReadOnlyList<TestDiagnosticEntry> Entries => _entries;

    public void Add(string code, string message, TestLayer? layer = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        _entries.Add(new TestDiagnosticEntry(code, message, layer));
    }

    public bool Contains(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return _entries.Any(entry => string.Equals(entry.Code, code, StringComparison.Ordinal));
    }
}
