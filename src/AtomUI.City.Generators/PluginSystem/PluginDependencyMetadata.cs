namespace AtomUI.City.Generators.PluginSystem;

public sealed class PluginDependencyMetadata
{
    public PluginDependencyMetadata(string pluginId, string? versionRange)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
        {
            throw new ArgumentException("Plugin dependency id cannot be empty.", nameof(pluginId));
        }

        PluginId = pluginId;
        VersionRange = versionRange;
    }

    public string PluginId { get; }

    public string? VersionRange { get; }
}
