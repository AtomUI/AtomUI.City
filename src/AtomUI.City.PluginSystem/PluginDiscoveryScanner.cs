using System.Text.Json;

namespace AtomUI.City.PluginSystem;

public static class PluginDiscoveryScanner
{
    public static PluginDiscoveryResult DiscoverInstalled(string pluginsRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginsRoot);

        var installedRoot = Path.Combine(pluginsRoot, PluginPackagePaths.InstalledDirectoryName);
        if (!Directory.Exists(installedRoot))
        {
            return new PluginDiscoveryResult([], []);
        }

        var plugins = new List<PluginDescriptor>();
        var diagnostics = new List<PluginDiagnostic>();

        foreach (var installRecordPath in EnumerateInstallRecords(installedRoot))
        {
            if (!File.Exists(installRecordPath))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.MissingInstallRecord,
                    $"Plugin install record '{installRecordPath}' was not found.",
                    Field: "installRecord",
                    Path: installRecordPath));
                continue;
            }

            PluginInstallation installation;
            try
            {
                installation = PluginInstallationReader.Read(installRecordPath);
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.InvalidInstallRecord,
                    $"Plugin install record '{installRecordPath}' could not be read: {exception.Message}",
                    Field: "installRecord",
                    Path: installRecordPath));
                continue;
            }

            var installedVersionPath = Path.GetDirectoryName(installRecordPath)!;
            var expectedRootPath = Path.Combine(installedVersionPath, PluginPackagePaths.RuntimeRootDirectoryName);
            if (!TryNormalizePath(installation.RootPath, out var actualRootPath))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.InvalidInstallRecord,
                    $"Plugin install record root path '{installation.RootPath}' is not a valid path.",
                    installation.PluginId,
                    "rootPath",
                    installRecordPath));
                continue;
            }

            var normalizedExpectedRootPath = NormalizePath(expectedRootPath);
            if (!string.Equals(actualRootPath, normalizedExpectedRootPath, StringComparison.Ordinal))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.InvalidInstallRecord,
                    $"Plugin install record root path '{installation.RootPath}' must match '{expectedRootPath}'.",
                    installation.PluginId,
                    "rootPath",
                    installRecordPath));
                continue;
            }

            var expectedManifestPath = PluginPackagePaths.GetManifestPath(expectedRootPath);
            if (!TryNormalizePath(installation.ManifestPath, out var actualManifestPath))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.InvalidInstallRecord,
                    $"Plugin install record manifest path '{installation.ManifestPath}' is not a valid path.",
                    installation.PluginId,
                    "manifestPath",
                    installRecordPath));
                continue;
            }

            var normalizedExpectedManifestPath = NormalizePath(expectedManifestPath);
            if (!string.Equals(actualManifestPath, normalizedExpectedManifestPath, StringComparison.Ordinal))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.InvalidInstallRecord,
                    $"Plugin install record manifest path '{installation.ManifestPath}' must match '{expectedManifestPath}'.",
                    installation.PluginId,
                    "manifestPath",
                    installRecordPath));
                continue;
            }

            if (!File.Exists(installation.ManifestPath))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.ManifestNotFound,
                    $"Installed plugin manifest '{installation.ManifestPath}' was not found.",
                    installation.PluginId,
                    "manifestPath",
                    installation.ManifestPath));
                continue;
            }

            PluginManifest manifest;
            try
            {
                manifest = PluginManifestReader.Read(installation.ManifestPath);
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.InvalidManifest,
                    $"Installed plugin manifest '{installation.ManifestPath}' could not be read: {exception.Message}",
                    installation.PluginId,
                    "manifest",
                    installation.ManifestPath));
                continue;
            }

            var manifestValidation = PluginManifestValidator.Validate(manifest);
            if (!manifestValidation.Succeeded)
            {
                diagnostics.AddRange(manifestValidation.Diagnostics);
                continue;
            }

            if (!string.Equals(manifest.PluginId, installation.PluginId, StringComparison.Ordinal))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.PluginIdMismatch,
                    $"Install record plugin id '{installation.PluginId}' does not match manifest plugin id '{manifest.PluginId}'.",
                    installation.PluginId,
                    "pluginId",
                    installRecordPath));
                continue;
            }

            if (!string.Equals(manifest.Version, installation.Version, StringComparison.Ordinal))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.PluginVersionMismatch,
                    $"Install record version '{installation.Version}' does not match manifest version '{manifest.Version}'.",
                    installation.PluginId,
                    "version",
                    installRecordPath));
                continue;
            }

            if (!string.Equals(manifest.PackageId, installation.PackageId, StringComparison.Ordinal))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.PluginPackageIdMismatch,
                    $"Install record package id '{installation.PackageId}' does not match manifest package id '{manifest.PackageId}'.",
                    installation.PluginId,
                    "packageId",
                    installRecordPath));
                continue;
            }

            var layoutValidation = PluginPackageLayoutValidator.Validate(installation.RootPath);
            if (!layoutValidation.Succeeded)
            {
                diagnostics.AddRange(layoutValidation.Diagnostics);
                continue;
            }

            plugins.Add(PluginDescriptor.FromManifest(manifest, installation.RootPath));
        }

        return new PluginDiscoveryResult(plugins, diagnostics);
    }

    private static IEnumerable<string> EnumerateInstallRecords(string installedRoot)
    {
        foreach (var pluginDirectory in Directory.EnumerateDirectories(installedRoot))
        {
            foreach (var versionDirectory in Directory.EnumerateDirectories(pluginDirectory))
            {
                var installRecordPath = Path.Combine(
                    versionDirectory,
                    PluginPackagePaths.InstallRecordFileName);
                yield return installRecordPath;
            }
        }
    }

    private static bool TryNormalizePath(string path, out string normalizedPath)
    {
        try
        {
            normalizedPath = NormalizePath(path);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            normalizedPath = string.Empty;
            return false;
        }
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}

public sealed class PluginDiscoveryResult
{
    public PluginDiscoveryResult(
        IReadOnlyList<PluginDescriptor> plugins,
        IReadOnlyList<PluginDiagnostic> diagnostics)
    {
        Plugins = Array.AsReadOnly(plugins.ToArray());
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public IReadOnlyList<PluginDescriptor> Plugins { get; }

    public IReadOnlyList<PluginDiagnostic> Diagnostics { get; }

    public bool Succeeded => Diagnostics.Count == 0;
}
