namespace AtomUI.City.Generators.Modularity;

public sealed class ModuleDependencyMetadata
{
    public ModuleDependencyMetadata(string typeName, bool optional)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Dependency type name cannot be empty.", nameof(typeName));
        }

        TypeName = typeName;
        Optional = optional;
    }

    public string TypeName { get; }

    public bool Optional { get; }
}
