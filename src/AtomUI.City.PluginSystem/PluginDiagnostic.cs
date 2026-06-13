namespace AtomUI.City.PluginSystem;

public sealed record PluginDiagnostic(
    string Code,
    string Message,
    string? PluginId = null,
    string? Field = null,
    string? Path = null);

public sealed class PluginValidationResult
{
    public PluginValidationResult(IReadOnlyList<PluginDiagnostic> diagnostics)
    {
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public IReadOnlyList<PluginDiagnostic> Diagnostics { get; }

    public bool Succeeded => Diagnostics.Count == 0;

    public static PluginValidationResult Success { get; } = new([]);
}
