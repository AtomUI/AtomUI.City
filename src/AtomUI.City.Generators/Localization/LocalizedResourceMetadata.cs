namespace AtomUI.City.Generators.Localization;

public sealed class LocalizedResourceMetadata
{
    public LocalizedResourceMetadata(
        string key,
        string packageId,
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

        Key = key;
        PackageId = packageId;
        Kind = kind;
        Scope = scope;
        Version = version;
        Critical = critical;
    }

    public string Key { get; }

    public string PackageId { get; }

    public LocalizedResourceMetadataKind Kind { get; }

    public ResourceScopeMetadata Scope { get; }

    public string? Version { get; }

    public bool Critical { get; }
}
