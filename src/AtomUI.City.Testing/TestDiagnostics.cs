using System.Collections.ObjectModel;

namespace AtomUI.City.Testing;

public sealed class TestDiagnostics
{
    private readonly List<TestDiagnosticEntry> _entries = [];
    private readonly ReadOnlyCollection<TestDiagnosticEntry> _readOnlyEntries;

    public TestDiagnostics()
    {
        _readOnlyEntries = new ReadOnlyCollection<TestDiagnosticEntry>(_entries);
    }

    public IReadOnlyList<TestDiagnosticEntry> Entries => _readOnlyEntries;

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
