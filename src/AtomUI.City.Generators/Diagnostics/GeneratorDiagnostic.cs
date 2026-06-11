namespace AtomUI.City.Generators.Diagnostics;

public sealed class GeneratorDiagnostic
{
    public GeneratorDiagnostic(GeneratorDiagnosticDefinition definition, string? message = null, string? target = null)
    {
        var diagnosticDefinition = definition ?? throw new ArgumentNullException(nameof(definition));

        Definition = diagnosticDefinition;
        Message = string.IsNullOrWhiteSpace(message) ? diagnosticDefinition.Message : message!;
        Target = target;
    }

    public GeneratorDiagnosticDefinition Definition { get; }

    public string Id => Definition.Id;

    public string Title => Definition.Title;

    public string Message { get; }

    public GeneratorDiagnosticSeverity Severity => Definition.Severity;

    public string? Target { get; }
}
