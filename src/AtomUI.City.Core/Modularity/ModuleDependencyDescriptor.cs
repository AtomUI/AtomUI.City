namespace AtomUI.City.Modularity;

public sealed class ModuleDependencyDescriptor
{
    public ModuleDependencyDescriptor(Type moduleType, bool optional)
    {
        ArgumentNullException.ThrowIfNull(moduleType);

        ModuleType = moduleType;
        Optional = optional;
    }

    public Type ModuleType { get; }

    public bool Optional { get; }
}
