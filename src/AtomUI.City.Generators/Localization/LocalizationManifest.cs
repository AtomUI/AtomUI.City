namespace AtomUI.City.Generators.Localization;

public sealed class LocalizationManifest
{
    public LocalizationManifest(
        IReadOnlyList<LanguagePackageManifestEntry> packages,
        IReadOnlyList<LocalizedResourceManifestEntry> resources,
        IReadOnlyList<string> supportedCultures,
        IReadOnlyList<CultureFallbackManifestEntry> fallbacks)
    {
        Packages = Array.AsReadOnly((packages ?? throw new ArgumentNullException(nameof(packages))).ToArray());
        Resources = Array.AsReadOnly((resources ?? throw new ArgumentNullException(nameof(resources))).ToArray());
        SupportedCultures = Array.AsReadOnly((supportedCultures ?? throw new ArgumentNullException(nameof(supportedCultures))).ToArray());
        Fallbacks = Array.AsReadOnly((fallbacks ?? throw new ArgumentNullException(nameof(fallbacks))).ToArray());
    }

    public IReadOnlyList<LanguagePackageManifestEntry> Packages { get; }

    public IReadOnlyList<LocalizedResourceManifestEntry> Resources { get; }

    public IReadOnlyList<string> SupportedCultures { get; }

    public IReadOnlyList<CultureFallbackManifestEntry> Fallbacks { get; }
}
