using System.Globalization;

namespace AtomUI.City.Localization;

public sealed class LanguagePackageDescriptor
{
    public LanguagePackageDescriptor(
        string packageId,
        CultureInfo culture,
        ResourceScope scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        PackageId = packageId;
        Culture = culture ?? throw new ArgumentNullException(nameof(culture));
        Scope = scope;
    }

    public string PackageId { get; }

    public CultureInfo Culture { get; }

    public ResourceScope Scope { get; }

    public LanguagePackageProviderKind ProviderKind { get; init; } =
        LanguagePackageProviderKind.InMemory;

    public CultureInfo? FallbackCulture { get; init; }

    public string? Location { get; init; }

    public string? ResourceBaseName { get; init; }

    public string? Version { get; init; }

    public string? Checksum { get; init; }

    public string? ContributionId { get; init; }
}
