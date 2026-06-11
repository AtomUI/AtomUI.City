namespace AtomUI.City.Localization;

public sealed class LanguagePackageLoadResult
{
    private LanguagePackageLoadResult(LanguagePackage? package, LocalizationError? error)
    {
        Package = package;
        Error = error;
    }

    public LanguagePackage? Package { get; }

    public LocalizationError? Error { get; }

    public bool Succeeded => Error is null;

    public static LanguagePackageLoadResult Success(LanguagePackage package)
    {
        ArgumentNullException.ThrowIfNull(package);

        return new LanguagePackageLoadResult(package, error: null);
    }

    public static LanguagePackageLoadResult Failed(LocalizationError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new LanguagePackageLoadResult(package: null, error);
    }
}
