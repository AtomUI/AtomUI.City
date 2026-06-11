namespace AtomUI.City.Modularity;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ModuleAttribute : Attribute
{
    public ModuleAttribute()
    {
    }

    public ModuleAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    public string? Name { get; }

    public string? Version { get; set; }

    public string? Description { get; set; }
}
