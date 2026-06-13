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
            var installation = PluginInstallationReader.Read(installRecordPath);
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
