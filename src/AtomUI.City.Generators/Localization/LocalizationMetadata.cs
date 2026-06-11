namespace AtomUI.City.Generators.Localization;

public sealed class LocalizationMetadata
{
    public LocalizationMetadata(
        IReadOnlyList<LanguagePackageMetadata> packages,
        IReadOnlyList<LocalizedResourceMetadata> resources)
    {
        Packages = packages ?? throw new ArgumentNullException(nameof(packages));
        Resources = resources ?? throw new ArgumentNullException(nameof(resources));
    }

    public IReadOnlyList<LanguagePackageMetadata> Packages { get; }

    public IReadOnlyList<LocalizedResourceMetadata> Resources { get; }
}
