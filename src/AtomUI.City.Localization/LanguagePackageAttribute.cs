namespace AtomUI.City.Localization;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class LanguagePackageAttribute : Attribute
{
    public LanguagePackageAttribute(string packageId, string culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);

        PackageId = packageId;
        Culture = culture;
    }

    public string PackageId { get; }

    public string Culture { get; }

    public ResourceScope Scope { get; set; } = ResourceScope.Module;

    public string? ResourceBaseName { get; set; }

    public string? FallbackCulture { get; set; }

    public string? Version { get; set; }

    public string? Checksum { get; set; }
}
