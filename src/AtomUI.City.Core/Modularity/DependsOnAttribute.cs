namespace AtomUI.City.Modularity;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DependsOnAttribute : Attribute
{
    public DependsOnAttribute(Type moduleType)
    {
        ArgumentNullException.ThrowIfNull(moduleType);

        ModuleType = moduleType;
    }

    public Type ModuleType { get; }

    public bool Optional { get; set; }
}
