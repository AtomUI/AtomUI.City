using System.Globalization;
using System.Text.Json;

namespace AtomUI.City.Localization;

internal static class LocPackReader
{
    public static LanguagePackageLoadResult Read(Stream stream, LanguagePackageDescriptor descriptor)
    {
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;
        var packageId = ReadRequiredString(root, "packageId");
        var cultureName = ReadRequiredString(root, "culture");

        if (!string.Equals(packageId, descriptor.PackageId, StringComparison.Ordinal))
        {
            return LanguagePackageLoadResult.Failed(
                new LocalizationError(
                    LocalizationErrorKind.PackageVersionMismatch,
                    $"Language package id '{packageId}' does not match descriptor '{descriptor.PackageId}'."));
        }

        var culture = CultureInfo.GetCultureInfo(cultureName);
        if (!string.Equals(culture.Name, descriptor.Culture.Name, StringComparison.OrdinalIgnoreCase))
        {
            return LanguagePackageLoadResult.Failed(
                new LocalizationError(
                    LocalizationErrorKind.PackageCultureMismatch,
                    $"Language package culture '{culture.Name}' does not match descriptor '{descriptor.Culture.Name}'."));
        }

        var resources = new Dictionary<string, string>(StringComparer.Ordinal);
        if (root.TryGetProperty("resources", out var resourcesElement) &&
            resourcesElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in resourcesElement.EnumerateObject())
            {
                resources[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }

        return LanguagePackageLoadResult.Success(LanguagePackage.Create(descriptor, resources));
    }

    private static string ReadRequiredString(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()!
            : throw new InvalidOperationException($"Localization pack property '{name}' is required.");
    }
}
