using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.DependencyInjection;

public sealed class ServiceRegistrationManifestResult
{
    public ServiceRegistrationManifestResult(
        ServiceRegistrationManifest manifest,
        IReadOnlyList<GeneratorDiagnostic> diagnostics)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
    }

    public ServiceRegistrationManifest Manifest { get; }

    public IReadOnlyList<GeneratorDiagnostic> Diagnostics { get; }
}
