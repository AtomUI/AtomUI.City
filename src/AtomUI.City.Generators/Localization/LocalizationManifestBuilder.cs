using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Localization;

public static class LocalizationManifestBuilder
{
    public static LocalizationManifestResult Build(
        IReadOnlyList<LanguagePackageMetadata> packages,
        IReadOnlyList<LocalizedResourceMetadata> resources)
    {
        if (packages is null)
        {
            throw new ArgumentNullException(nameof(packages));
        }

        if (resources is null)
        {
            throw new ArgumentNullException(nameof(resources));
        }

        var diagnostics = new List<GeneratorDiagnostic>();
        var packagesById = new Dictionary<string, LanguagePackageMetadata>(StringComparer.Ordinal);

        foreach (var package in packages)
        {
            if (packagesById.ContainsKey(package.PackageId))
            {
                diagnostics.Add(new GeneratorDiagnostic(
                    GeneratorDiagnostics.InvalidManifestInput,
                    $"Language package '{package.PackageId}' is declared more than once.",
                    package.PackageId));
                continue;
            }

            packagesById.Add(package.PackageId, package);
        }

        var resourceKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var resource in resources)
        {
            if (!packagesById.ContainsKey(resource.PackageId))
            {
                diagnostics.Add(new GeneratorDiagnostic(
                    GeneratorDiagnostics.InvalidManifestInput,
                    $"Localized resource '{resource.Key}' references missing package '{resource.PackageId}'.",
                    resource.Key));
                continue;
            }

            var resourceKey = string.Join("|", resource.PackageId, resource.Scope, resource.Key);

            if (!resourceKeys.Add(resourceKey))
            {
                diagnostics.Add(new GeneratorDiagnostic(
                    GeneratorDiagnostics.InvalidManifestInput,
                    $"Localized resource '{resource.Key}' is declared more than once in package '{resource.PackageId}'.",
                    resource.Key));
            }
        }

        var supportedCultures = new HashSet<string>(
            packages.Select(package => package.Culture),
            StringComparer.Ordinal);

        foreach (var package in packages)
        {
            if (string.IsNullOrWhiteSpace(package.FallbackCulture) ||
                supportedCultures.Contains(package.FallbackCulture!))
            {
                continue;
            }

            diagnostics.Add(new GeneratorDiagnostic(
                GeneratorDiagnostics.InvalidManifestInput,
                $"Language package '{package.PackageId}' declares fallback culture '{package.FallbackCulture}' without a matching language package.",
                package.PackageId));
        }

        if (diagnostics.Count > 0)
        {
            return new LocalizationManifestResult(CreateEmptyManifest(), diagnostics);
        }

        var packageEntries = packages
            .Select(package => new LanguagePackageManifestEntry(
                package.PackageId,
                package.Culture,
                package.Scope,
                package.ResourceBaseName,
                package.FallbackCulture,
                package.Version,
                package.Checksum))
            .OrderBy(package => package.PackageId, StringComparer.Ordinal)
            .ToArray();
        var resourceEntries = resources
            .Select(resource =>
            {
                var package = packagesById[resource.PackageId];

                return new LocalizedResourceManifestEntry(
                    resource.Key,
                    resource.PackageId,
                    package.Culture,
                    resource.Kind,
                    resource.Scope,
                    resource.Version,
                    resource.Critical);
            })
            .OrderBy(resource => resource.PackageId, StringComparer.Ordinal)
            .ThenBy(resource => resource.Key, StringComparer.Ordinal)
            .ToArray();
        var manifestSupportedCultures = packages
            .Select(package => package.Culture)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(culture => culture, StringComparer.Ordinal)
            .ToArray();
        var fallbacks = packages
            .Where(package => !string.IsNullOrWhiteSpace(package.FallbackCulture))
            .Select(package => new CultureFallbackManifestEntry(package.Culture, package.FallbackCulture!))
            .OrderBy(fallback => fallback.Culture, StringComparer.Ordinal)
            .ThenBy(fallback => fallback.FallbackCulture, StringComparer.Ordinal)
            .ToArray();

        return new LocalizationManifestResult(
            new LocalizationManifest(packageEntries, resourceEntries, manifestSupportedCultures, fallbacks),
            diagnostics);
    }

    private static LocalizationManifest CreateEmptyManifest()
    {
        return new LocalizationManifest([], [], [], []);
    }
}
