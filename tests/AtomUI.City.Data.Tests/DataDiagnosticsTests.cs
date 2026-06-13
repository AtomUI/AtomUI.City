using AtomUI.City.Data;

namespace AtomUI.City.Data.Tests;

public sealed class DataDiagnosticsTests
{
    [Fact]
    public void RecordsSnapshotRejectsExternalListMutation()
    {
        var diagnostics = new InMemoryDataDiagnostics();
        diagnostics.Write(new DataDiagnosticRecord("AUCDATA999", "First", DataDiagnosticSeverity.Info));
        var records = Assert.IsAssignableFrom<IList<DataDiagnosticRecord>>(diagnostics.Records);

        Assert.Throws<NotSupportedException>(() => records[0] = new DataDiagnosticRecord(
            "AUCDATA998",
            "Changed",
            DataDiagnosticSeverity.Error));
        Assert.Equal("AUCDATA999", diagnostics.Records[0].Code);
    }
}
