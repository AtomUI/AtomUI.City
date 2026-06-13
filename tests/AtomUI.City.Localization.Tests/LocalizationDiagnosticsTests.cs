using AtomUI.City.Localization;

namespace AtomUI.City.Localization.Tests;

public sealed class LocalizationDiagnosticsTests
{
    [Fact]
    public void RecordsSnapshotRejectsExternalListMutation()
    {
        var diagnostics = new InMemoryLocalizationDiagnostics();
        diagnostics.Write(new LocalizationDiagnosticRecord("AUCLOC999", "First", LocalizationDiagnosticSeverity.Info));
        var records = Assert.IsAssignableFrom<IList<LocalizationDiagnosticRecord>>(diagnostics.Records);

        Assert.Throws<NotSupportedException>(() => records[0] = new LocalizationDiagnosticRecord(
            "AUCLOC998",
            "Changed",
            LocalizationDiagnosticSeverity.Error));
        Assert.Equal("AUCLOC999", diagnostics.Records[0].Code);
    }
}
