namespace AtomUI.City.Generators.Diagnostics;

public sealed class GeneratorDiagnosticDefinition
{
    public GeneratorDiagnosticDefinition(string id, string title, string message, GeneratorDiagnosticSeverity severity)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Diagnostic id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Diagnostic title cannot be empty.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Diagnostic message cannot be empty.", nameof(message));
        }

        Id = id;
        Title = title;
        Message = message;
        Severity = severity;
    }

    public string Id { get; }

    public string Title { get; }

    public string Message { get; }

    public GeneratorDiagnosticSeverity Severity { get; }
}
