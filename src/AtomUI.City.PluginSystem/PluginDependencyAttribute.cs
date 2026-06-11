namespace AtomUI.City.PluginSystem;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class PluginDependencyAttribute : Attribute
{
    public PluginDependencyAttribute(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        PluginId = pluginId;
    }

    public string PluginId { get; }

    public string? VersionRange { get; set; }
}
