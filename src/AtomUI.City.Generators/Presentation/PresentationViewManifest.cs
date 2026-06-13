namespace AtomUI.City.Generators.Presentation;

public sealed class PresentationViewManifest
{
    public PresentationViewManifest(IReadOnlyList<PresentationViewManifestEntry> views)
    {
        Views = Array.AsReadOnly((views ?? throw new ArgumentNullException(nameof(views))).ToArray());
    }

    public IReadOnlyList<PresentationViewManifestEntry> Views { get; }
}
