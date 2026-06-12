namespace AtomUI.City.Generators.Presentation;

public sealed class PresentationViewManifest
{
    public PresentationViewManifest(IReadOnlyList<PresentationViewManifestEntry> views)
    {
        Views = views ?? throw new ArgumentNullException(nameof(views));
    }

    public IReadOnlyList<PresentationViewManifestEntry> Views { get; }
}
