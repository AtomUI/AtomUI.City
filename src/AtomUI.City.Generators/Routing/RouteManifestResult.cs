using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Routing;

public sealed class RouteManifestResult
{
    public RouteManifestResult(RouteManifest manifest, IReadOnlyList<GeneratorDiagnostic> diagnostics)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Diagnostics = Array.AsReadOnly((diagnostics ?? throw new ArgumentNullException(nameof(diagnostics))).ToArray());
    }

    public RouteManifest Manifest { get; }

    public IReadOnlyList<GeneratorDiagnostic> Diagnostics { get; }
}
