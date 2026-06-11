namespace AtomUI.City.Localization;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class LocalizedResourceAttribute : Attribute
{
    public LocalizedResourceAttribute(string key, string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        Key = key;
        PackageId = packageId;
    }

    public string Key { get; }

    public string PackageId { get; }

    public LocalizedResourceKind Kind { get; set; } = LocalizedResourceKind.String;

    public ResourceScope Scope { get; set; } = ResourceScope.Module;

    public string? Version { get; set; }

    public bool Critical { get; set; }
}
