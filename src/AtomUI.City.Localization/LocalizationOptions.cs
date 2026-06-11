namespace AtomUI.City.Localization;

public sealed class LocalizationOptions
{
    public IList<LanguagePackageDescriptor> LanguagePackages { get; } =
        new List<LanguagePackageDescriptor>();
}
