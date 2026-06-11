namespace AtomUI.City.Generators.Localization;

public sealed class LanguagePackageMetadata
{
    public LanguagePackageMetadata(
        string packageId,
        string culture,
        ResourceScopeMetadata scope,
        string? resourceBaseName,
        string? fallbackCulture,
        string? version,
        string? checksum)
    {
        if (string.IsNullOrWhiteSpace(packageId))
        {
            throw new ArgumentException("Package id cannot be empty.", nameof(packageId));
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Culture cannot be empty.", nameof(culture));
        }

        PackageId = packageId;
        Culture = culture;
        Scope = scope;
        ResourceBaseName = resourceBaseName;
        FallbackCulture = fallbackCulture;
        Version = version;
        Checksum = checksum;
    }

    public string PackageId { get; }

    public string Culture { get; }

    public ResourceScopeMetadata Scope { get; }

    public string? ResourceBaseName { get; }

    public string? FallbackCulture { get; }

    public string? Version { get; }

    public string? Checksum { get; }
}
