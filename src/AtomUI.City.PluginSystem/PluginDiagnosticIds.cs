namespace AtomUI.City.PluginSystem;

public static class PluginDiagnosticIds
{
    public const string ManifestNotFound = "AUCPLG0000";
    public const string MissingPluginId = "AUCPLG0001";
    public const string MainAssemblyNotFound = "AUCPLG0002";
    public const string PluginIdMismatch = "AUCPLG0003";
    public const string UnsupportedManifestSchema = "AUCPLG0004";
    public const string RequiredContributionManifestNotFound = "AUCPLG0005";
    public const string InvalidMainAssembly = "AUCPLG0006";
    public const string PluginAlreadyInstalled = "AUCPLG0007";
    public const string PluginDependencyMissing = "AUCPLG0008";
    public const string PluginDependencyCycle = "AUCPLG0009";
}
