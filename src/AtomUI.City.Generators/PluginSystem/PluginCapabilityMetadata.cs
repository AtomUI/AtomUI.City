namespace AtomUI.City.Generators.PluginSystem;

public sealed class PluginCapabilityMetadata
{
    public PluginCapabilityMetadata(string name, IReadOnlyList<string> scope)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Capability name cannot be empty.", nameof(name));
        }

        Name = name;
        Scope = Array.AsReadOnly((scope ?? throw new ArgumentNullException(nameof(scope))).ToArray());
    }

    public string Name { get; }

    public IReadOnlyList<string> Scope { get; }
}
