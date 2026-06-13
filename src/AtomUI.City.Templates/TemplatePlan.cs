using System.Collections.ObjectModel;

namespace AtomUI.City.Templates;

public sealed class TemplatePlan
{
    public TemplatePlan(
        string operationId,
        string command,
        IReadOnlyDictionary<string, object?> inputs,
        IReadOnlyList<TemplateChange> changes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        OperationId = operationId;
        Command = command;
        Inputs = new ReadOnlyDictionary<string, object?>(
            new Dictionary<string, object?>(inputs, StringComparer.Ordinal));
        Changes = Array.AsReadOnly(changes.ToArray());
    }

    public string SchemaVersion { get; } = "1.0";

    public string OperationId { get; }

    public string Command { get; }

    public IReadOnlyDictionary<string, object?> Inputs { get; }

    public IReadOnlyList<TemplateChange> Changes { get; }

    public IReadOnlyList<string> BuildTargets { get; } = [];

    public IReadOnlyList<string> TestTargets { get; } = [];

    public IReadOnlyList<string> DocsRequired { get; } = [];

    public IReadOnlyList<string> Risks { get; } = [];

    public IReadOnlyList<string> Rollback { get; } = [];
}
