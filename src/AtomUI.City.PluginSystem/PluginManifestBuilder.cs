namespace AtomUI.City.PluginSystem;

public static class PluginManifestBuilder
{
    public static PluginManifest Minimal(
        string pluginId,
        string packageId,
        string version,
        string mainAssembly = "Plugin.dll",
        IReadOnlyList<PluginCapabilityDescriptor>? capabilities = null,
        IReadOnlyList<PluginContributionDescriptor>? contributions = null,
        IReadOnlyList<PluginDependencyDescriptor>? dependencies = null,
        IReadOnlyList<PluginModuleDescriptor>? modules = null)
    {
        return new PluginManifest(
            schemaVersion: "1.0",
            pluginId,
            packageId,
            version,
            displayNameKey: $"{pluginId}.DisplayName",
            descriptionKey: null,
            publisher: null,
            mainAssembly,
            targetFramework: "net10.0",
            pluginApiVersion: "1.0",
            minHostVersion: "1.0.0",
            maxHostVersion: null,
            unloadable: true,
            aotCompatible: false,
            capabilities ?? [],
            contributions ?? [],
            dependencies ?? [],
            modules ?? []);
    }
}
