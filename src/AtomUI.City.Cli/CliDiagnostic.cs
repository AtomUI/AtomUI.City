namespace AtomUI.City.Cli;

public sealed record CliDiagnostic(
    string Code,
    string Message,
    string Severity,
    string? SuggestedAction = null,
    string? DocumentationLink = null)
{
    public static CliDiagnostic Error(string code, string message)
    {
        return new CliDiagnostic(code, message, "Error");
    }
}
