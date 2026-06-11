using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.PluginSystem;

public sealed class PluginManifestResult
{
    public PluginManifestResult(PluginManifest manifest, IReadOnlyList<GeneratorDiagnostic> diagnostics)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
    }

    public PluginManifest Manifest { get; }

    public IReadOnlyList<GeneratorDiagnostic> Diagnostics { get; }
}
