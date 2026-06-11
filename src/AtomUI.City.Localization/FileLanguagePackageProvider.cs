namespace AtomUI.City.Localization;

public sealed class FileLanguagePackageProvider : ILanguagePackageProvider
{
    public LanguagePackageProviderKind Kind => LanguagePackageProviderKind.File;

    public async ValueTask<LanguagePackageLoadResult> LoadAsync(
        LanguagePackageDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (string.IsNullOrWhiteSpace(descriptor.Location) || !File.Exists(descriptor.Location))
        {
            return LanguagePackageLoadResult.Failed(
                new LocalizationError(
                    LocalizationErrorKind.PackageNotFound,
                    $"File language package '{descriptor.Location}' was not found."));
        }

        try
        {
            await using var stream = File.OpenRead(descriptor.Location);

            return LocPackReader.Read(stream, descriptor);
        }
        catch (Exception exception)
        {
            return LanguagePackageLoadResult.Failed(
                new LocalizationError(
                    LocalizationErrorKind.PackageLoadFailed,
                    exception.Message,
                    Exception: exception));
        }
    }
}
