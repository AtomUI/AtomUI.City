namespace AtomUI.City.Localization;

public sealed record LocalizationDiagnosticRecord(
    string Code,
    string Message,
    LocalizationDiagnosticSeverity Severity,
    string? CultureName = null,
    string? FallbackCultureName = null,
    string? ResourceKey = null,
    string? PackageId = null,
    ResourceScope? Scope = null,
    long? CultureRevision = null,
    LocalizationErrorKind? ErrorKind = null);

public enum LocalizationDiagnosticSeverity
{
    Trace,
    Info,
    Warning,
    Error,
}
