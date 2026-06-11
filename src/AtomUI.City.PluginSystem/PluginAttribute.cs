namespace AtomUI.City.PluginSystem;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class PluginAttribute : Attribute
{
    public PluginAttribute(string pluginId, string packageId, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        PluginId = pluginId;
        PackageId = packageId;
        Version = version;
    }

    public string PluginId { get; }

    public string PackageId { get; }

    public string Version { get; }

    public string? DisplayNameKey { get; set; }

    public string? DescriptionKey { get; set; }

    public string? Publisher { get; set; }

    public string? MainAssembly { get; set; }

    public string? TargetFramework { get; set; }

    public string? PluginApiVersion { get; set; }

    public string? MinHostVersion { get; set; }

    public string? MaxHostVersion { get; set; }

    public bool Unloadable { get; set; } = true;

    public bool AotCompatible { get; set; }
}
