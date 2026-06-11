using System.Reflection;

namespace AtomUI.City.Localization;

public sealed class AssemblyLanguagePackageProvider : ILanguagePackageProvider
{
    public LanguagePackageProviderKind Kind => LanguagePackageProviderKind.Assembly;

    public ValueTask<LanguagePackageLoadResult> LoadAsync(
        LanguagePackageDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(descriptor.Location))
        {
            return ValueTask.FromResult(
                LanguagePackageLoadResult.Failed(
                    new LocalizationError(
                        LocalizationErrorKind.PackageNotFound,
                        "Assembly language package location is required.")));
        }

        if (string.IsNullOrWhiteSpace(descriptor.ResourceBaseName))
        {
            return ValueTask.FromResult(
                LanguagePackageLoadResult.Failed(
                    new LocalizationError(
                        LocalizationErrorKind.PackageNotFound,
                        "Assembly language package resource name is required.")));
        }

        try
        {
            var assembly = Assembly.LoadFrom(descriptor.Location);
            var resourceName = ResolveResourceName(assembly, descriptor.ResourceBaseName);

            if (resourceName is null)
            {
                return ValueTask.FromResult(
                    LanguagePackageLoadResult.Failed(
                        new LocalizationError(
                            LocalizationErrorKind.PackageNotFound,
                            $"Embedded localization resource '{descriptor.ResourceBaseName}' was not found.")));
            }

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                return ValueTask.FromResult(
                    LanguagePackageLoadResult.Failed(
                        new LocalizationError(
                            LocalizationErrorKind.PackageNotFound,
                            $"Embedded localization resource '{resourceName}' was not found.")));
            }

            return ValueTask.FromResult(LocPackReader.Read(stream, descriptor));
        }
        catch (Exception exception)
        {
            return ValueTask.FromResult(
                LanguagePackageLoadResult.Failed(
                    new LocalizationError(
                        LocalizationErrorKind.PackageLoadFailed,
                        exception.Message,
                        Exception: exception)));
        }
    }

    private static string? ResolveResourceName(Assembly assembly, string resourceBaseName)
    {
        var names = assembly.GetManifestResourceNames();

        return names.FirstOrDefault(name => string.Equals(name, resourceBaseName, StringComparison.Ordinal))
            ?? names.FirstOrDefault(name => name.EndsWith(resourceBaseName, StringComparison.Ordinal));
    }
}
