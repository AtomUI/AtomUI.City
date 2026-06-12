using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Presentation;

public sealed class PresentationViewManifestResult
{
    public PresentationViewManifestResult(
        PresentationViewManifest manifest,
        IReadOnlyList<GeneratorDiagnostic> diagnostics)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
    }

    public PresentationViewManifest Manifest { get; }

    public IReadOnlyList<GeneratorDiagnostic> Diagnostics { get; }
}
