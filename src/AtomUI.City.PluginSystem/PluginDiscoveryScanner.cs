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
            if (!AreSamePath(installation.RootPath, expectedRootPath))
            {
                diagnostics.Add(new PluginDiagnostic(
                    PluginDiagnosticIds.InvalidInstallRecord,
                    $"Plugin install record root path '{installation.RootPath}' must match '{expectedRootPath}'.",
                    installation.PluginId,
                    "rootPath",
                    installRecordPath));
                continue;
            }

            var expectedManifestPath = PluginPackagePaths.GetManifestPath(installation.RootPath);
            if (!AreSamePath(installation.ManifestPath, expectedManifestPath))
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

    private static bool AreSamePath(string left, string right)
    {
        return string.Equals(NormalizePath(left), NormalizePath(right), StringComparison.Ordinal);
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
