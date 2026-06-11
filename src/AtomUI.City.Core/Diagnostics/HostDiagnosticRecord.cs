using AtomUI.City.Lifecycle;

namespace AtomUI.City.Diagnostics;

public sealed record HostDiagnosticRecord(
    string Code,
    string Message,
    HostDiagnosticSeverity Severity,
    string? ScopeId = null,
    LifecycleStage? Stage = null);
