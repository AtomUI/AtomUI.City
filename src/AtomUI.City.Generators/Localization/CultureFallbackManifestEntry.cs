namespace AtomUI.City.Generators.Localization;

public sealed class CultureFallbackManifestEntry
{
    public CultureFallbackManifestEntry(string culture, string fallbackCulture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Culture cannot be empty.", nameof(culture));
        }

        if (string.IsNullOrWhiteSpace(fallbackCulture))
        {
            throw new ArgumentException("Fallback culture cannot be empty.", nameof(fallbackCulture));
        }

        Culture = culture;
        FallbackCulture = fallbackCulture;
    }

    public string Culture { get; }

    public string FallbackCulture { get; }
}
