namespace AtomUI.City.Generators.Localization;

public sealed class LocalizedResourceManifestEntry
{
    public LocalizedResourceManifestEntry(
        string key,
        string packageId,
        string culture,
        LocalizedResourceMetadataKind kind,
        ResourceScopeMetadata scope,
        string? version,
        bool critical)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Resource key cannot be empty.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(packageId))
        {
            throw new ArgumentException("Package id cannot be empty.", nameof(packageId));
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Culture cannot be empty.", nameof(culture));
        }

        Key = key;
        PackageId = packageId;
        Culture = culture;
        Kind = kind;
        Scope = scope;
        Version = version;
        Critical = critical;
    }

    public string Key { get; }

    public string PackageId { get; }

    public string Culture { get; }

    public LocalizedResourceMetadataKind Kind { get; }

    public ResourceScopeMetadata Scope { get; }

    public string? Version { get; }

    public bool Critical { get; }
}
