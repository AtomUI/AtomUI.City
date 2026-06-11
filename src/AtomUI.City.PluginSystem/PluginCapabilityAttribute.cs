namespace AtomUI.City.PluginSystem;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class PluginCapabilityAttribute : Attribute
{
    public PluginCapabilityAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    public string Name { get; }

    public string[] Scope { get; set; } = [];
}
