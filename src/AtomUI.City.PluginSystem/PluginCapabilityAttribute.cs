namespace AtomUI.City.PluginSystem;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class PluginCapabilityAttribute : Attribute
{
    private string[] _scope = [];

    public PluginCapabilityAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    public string Name { get; }

    public string[] Scope
    {
        get => _scope.ToArray();
        set => _scope = value?.ToArray() ?? [];
    }
}
