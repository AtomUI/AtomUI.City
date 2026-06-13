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

            var manifest = PluginManifestReader.Read(installation.ManifestPath);

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
                if (File.Exists(installRecordPath))
                {
                    yield return installRecordPath;
                }
            }
        }
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
