namespace AtomUI.City.Generators.PluginSystem;

public sealed class PluginManifest
{
    public PluginManifest(
        string schemaVersion,
        string pluginId,
        string packageId,
        string version,
        string displayNameKey,
        string? descriptionKey,
        string? publisher,
        string mainAssembly,
        string targetFramework,
        string pluginApiVersion,
        string minHostVersion,
        string? maxHostVersion,
        bool unloadable,
        bool aotCompatible,
        IReadOnlyList<PluginCapabilityManifestEntry> capabilities,
        IReadOnlyList<PluginContributionManifestEntry> contributions,
        IReadOnlyList<PluginDependencyManifestEntry> dependencies)
    {
        SchemaVersion = schemaVersion;
        PluginId = pluginId;
        PackageId = packageId;
        Version = version;
        DisplayNameKey = displayNameKey;
        DescriptionKey = descriptionKey;
        Publisher = publisher;
        MainAssembly = mainAssembly;
        TargetFramework = targetFramework;
        PluginApiVersion = pluginApiVersion;
        MinHostVersion = minHostVersion;
        MaxHostVersion = maxHostVersion;
        Unloadable = unloadable;
        AotCompatible = aotCompatible;
        Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
        Contributions = contributions ?? throw new ArgumentNullException(nameof(contributions));
        Dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
    }

    public string SchemaVersion { get; }

    public string PluginId { get; }

    public string PackageId { get; }

    public string Version { get; }

    public string DisplayNameKey { get; }

    public string? DescriptionKey { get; }

    public string? Publisher { get; }

    public string MainAssembly { get; }

    public string TargetFramework { get; }

    public string PluginApiVersion { get; }

    public string MinHostVersion { get; }

    public string? MaxHostVersion { get; }

    public bool Unloadable { get; }

    public bool AotCompatible { get; }

    public IReadOnlyList<PluginCapabilityManifestEntry> Capabilities { get; }

    public IReadOnlyList<PluginContributionManifestEntry> Contributions { get; }

    public IReadOnlyList<PluginDependencyManifestEntry> Dependencies { get; }
}
