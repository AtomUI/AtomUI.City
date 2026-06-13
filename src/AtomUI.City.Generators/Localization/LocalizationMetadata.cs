namespace AtomUI.City.Generators.Localization;

public sealed class LocalizationMetadata
{
    public LocalizationMetadata(
        IReadOnlyList<LanguagePackageMetadata> packages,
        IReadOnlyList<LocalizedResourceMetadata> resources)
    {
        Packages = Array.AsReadOnly((packages ?? throw new ArgumentNullException(nameof(packages))).ToArray());
        Resources = Array.AsReadOnly((resources ?? throw new ArgumentNullException(nameof(resources))).ToArray());
    }

    public IReadOnlyList<LanguagePackageMetadata> Packages { get; }

    public IReadOnlyList<LocalizedResourceMetadata> Resources { get; }
}
