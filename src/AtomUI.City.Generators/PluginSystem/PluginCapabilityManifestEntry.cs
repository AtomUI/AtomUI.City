namespace AtomUI.City.Generators.PluginSystem;

public sealed class PluginCapabilityManifestEntry
{
    public PluginCapabilityManifestEntry(string name, IReadOnlyList<string> scope)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Capability name cannot be empty.", nameof(name));
        }

        Name = name;
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
    }

    public string Name { get; }

    public IReadOnlyList<string> Scope { get; }
}
