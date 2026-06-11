using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Localization;

public sealed class LocalizationManifestResult
{
    public LocalizationManifestResult(
        LocalizationManifest manifest,
        IReadOnlyList<GeneratorDiagnostic> diagnostics)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
    }

    public LocalizationManifest Manifest { get; }

    public IReadOnlyList<GeneratorDiagnostic> Diagnostics { get; }
}
