namespace AtomUI.City.Generators.Localization;

public sealed class LocalizationManifest
{
    public LocalizationManifest(
        IReadOnlyList<LanguagePackageManifestEntry> packages,
        IReadOnlyList<LocalizedResourceManifestEntry> resources,
        IReadOnlyList<string> supportedCultures,
        IReadOnlyList<CultureFallbackManifestEntry> fallbacks)
    {
        Packages = packages ?? throw new ArgumentNullException(nameof(packages));
        Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        SupportedCultures = supportedCultures ?? throw new ArgumentNullException(nameof(supportedCultures));
        Fallbacks = fallbacks ?? throw new ArgumentNullException(nameof(fallbacks));
    }

    public IReadOnlyList<LanguagePackageManifestEntry> Packages { get; }

    public IReadOnlyList<LocalizedResourceManifestEntry> Resources { get; }

    public IReadOnlyList<string> SupportedCultures { get; }

    public IReadOnlyList<CultureFallbackManifestEntry> Fallbacks { get; }
}
