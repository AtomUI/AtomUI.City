namespace AtomUI.City.Generators.PluginSystem;

public sealed class PluginContributionManifestMetadata
{
    public PluginContributionManifestMetadata(string type, string path, bool required)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Contribution manifest type cannot be empty.", nameof(type));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Contribution manifest path cannot be empty.", nameof(path));
        }

        Type = type;
        Path = path;
        Required = required;
    }

    public string Type { get; }

    public string Path { get; }

    public bool Required { get; }
}
