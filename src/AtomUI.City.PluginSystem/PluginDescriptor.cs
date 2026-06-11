namespace AtomUI.City.PluginSystem;

public sealed class PluginDescriptor
{
    private PluginDescriptor(PluginManifest manifest, string rootPath)
    {
        Manifest = manifest;
        RootPath = rootPath;
    }

    public PluginManifest Manifest { get; }

    public string RootPath { get; }

    public string PluginId => Manifest.PluginId;

    public string PackageId => Manifest.PackageId;

    public string Version => Manifest.Version;

    public string MainAssemblyPath => PluginPackagePaths.GetMainAssemblyPath(
        RootPath,
        Manifest.TargetFramework,
        Manifest.MainAssembly);

    public static PluginDescriptor FromManifest(PluginManifest manifest, string rootPath)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        return new PluginDescriptor(manifest, rootPath);
    }
}
