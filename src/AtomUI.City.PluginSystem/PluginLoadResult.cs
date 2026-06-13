namespace AtomUI.City.PluginSystem;

public sealed class PluginLoadResult
{
    private PluginLoadResult(
        PluginRuntime? runtime,
        IReadOnlyList<PluginDiagnostic> diagnostics)
    {
        Runtime = runtime!;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public PluginRuntime Runtime { get; }

    public IReadOnlyList<PluginDiagnostic> Diagnostics { get; }

    public bool Succeeded => Diagnostics.Count == 0;

    public static PluginLoadResult Success(PluginRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        return new PluginLoadResult(runtime, []);
    }

    public static PluginLoadResult Failed(IReadOnlyList<PluginDiagnostic> diagnostics)
    {
        return new PluginLoadResult(null, diagnostics);
    }
}
