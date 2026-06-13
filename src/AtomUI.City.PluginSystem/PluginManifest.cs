namespace AtomUI.City.PluginSystem;

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
        IReadOnlyList<PluginCapabilityDescriptor> capabilities,
        IReadOnlyList<PluginContributionDescriptor> contributions,
        IReadOnlyList<PluginDependencyDescriptor> dependencies,
        IReadOnlyList<PluginModuleDescriptor> modules)
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
        Capabilities = Array.AsReadOnly(capabilities.ToArray());
        Contributions = Array.AsReadOnly(contributions.ToArray());
        Dependencies = Array.AsReadOnly(dependencies.ToArray());
        Modules = Array.AsReadOnly(modules.ToArray());
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

    public IReadOnlyList<PluginCapabilityDescriptor> Capabilities { get; }

    public IReadOnlyList<PluginContributionDescriptor> Contributions { get; }

    public IReadOnlyList<PluginDependencyDescriptor> Dependencies { get; }

    public IReadOnlyList<PluginModuleDescriptor> Modules { get; }
}

public sealed record PluginCapabilityDescriptor(string Name, IReadOnlyList<string> Scope);

public sealed record PluginContributionDescriptor(string Type, string Path, bool Required);

public sealed record PluginDependencyDescriptor(string PluginId, string? VersionRange);

public sealed record PluginModuleDescriptor(string Name, string TypeName, IReadOnlyList<string>? Dependencies = null);
