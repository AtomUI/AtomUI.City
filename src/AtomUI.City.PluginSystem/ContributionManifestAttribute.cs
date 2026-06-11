namespace AtomUI.City.PluginSystem;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ContributionManifestAttribute : Attribute
{
    public ContributionManifestAttribute(string type, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Type = type;
        Path = path;
    }

    public string Type { get; }

    public string Path { get; }

    public bool Required { get; set; }
}
