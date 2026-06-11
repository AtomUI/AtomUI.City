namespace AtomUI.City.PluginSystem;

public static class PluginPackagePaths
{
    public const string ManifestRelativePath = "atomui-city/plugin.json";
    public const string InstalledDirectoryName = "installed";
    public const string StagingDirectoryName = "staging";
    public const string RuntimeRootDirectoryName = "root";
    public const string InstallRecordFileName = "install.json";

    public static string GetManifestPath(string rootPath)
    {
        return CombinePackagePath(rootPath, ManifestRelativePath);
    }

    public static string GetMainAssemblyPath(
        string rootPath,
        string targetFramework,
        string mainAssembly)
    {
        return Path.Combine(rootPath, "lib", targetFramework, mainAssembly);
    }

    public static string GetInstalledVersionPath(
        string pluginsRoot,
        string pluginId,
        string version)
    {
        return Path.Combine(pluginsRoot, InstalledDirectoryName, pluginId, version);
    }

    public static string GetInstalledRootPath(
        string pluginsRoot,
        string pluginId,
        string version)
    {
        return Path.Combine(
            GetInstalledVersionPath(pluginsRoot, pluginId, version),
            RuntimeRootDirectoryName);
    }

    public static string GetInstallRecordPath(
        string pluginsRoot,
        string pluginId,
        string version)
    {
        return Path.Combine(
            GetInstalledVersionPath(pluginsRoot, pluginId, version),
            InstallRecordFileName);
    }

    internal static string CombinePackagePath(string rootPath, string relativePath)
    {
        return Path.Combine([rootPath, .. relativePath.Split('/')]);
    }
}
