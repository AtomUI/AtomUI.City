namespace AtomUI.City.Diagnostics;

public interface IHostDiagnostics
{
    IReadOnlyList<HostDiagnosticRecord> Records { get; }

    void Write(HostDiagnosticRecord record);
}
