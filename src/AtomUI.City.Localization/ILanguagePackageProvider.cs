namespace AtomUI.City.Localization;

public interface ILanguagePackageProvider
{
    LanguagePackageProviderKind Kind { get; }

    ValueTask<LanguagePackageLoadResult> LoadAsync(
        LanguagePackageDescriptor descriptor,
        CancellationToken cancellationToken = default);
}
