namespace AtomUI.City.Generators.PluginSystem;

public sealed class PluginMetadata
{
    public PluginMetadata(
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
        IReadOnlyList<PluginCapabilityMetadata> capabilities,
        IReadOnlyList<PluginContributionManifestMetadata> contributions,
        IReadOnlyList<PluginDependencyMetadata> dependencies)
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
        Capabilities = Array.AsReadOnly((capabilities ?? throw new ArgumentNullException(nameof(capabilities))).ToArray());
        Contributions = Array.AsReadOnly((contributions ?? throw new ArgumentNullException(nameof(contributions))).ToArray());
        Dependencies = Array.AsReadOnly((dependencies ?? throw new ArgumentNullException(nameof(dependencies))).ToArray());
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

    public IReadOnlyList<PluginCapabilityMetadata> Capabilities { get; }

    public IReadOnlyList<PluginContributionManifestMetadata> Contributions { get; }

    public IReadOnlyList<PluginDependencyMetadata> Dependencies { get; }
}
