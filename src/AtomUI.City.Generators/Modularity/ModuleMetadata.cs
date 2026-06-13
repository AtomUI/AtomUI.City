namespace AtomUI.City.Generators.Modularity;

public sealed class ModuleMetadata
{
    public ModuleMetadata(
        string name,
        string typeName,
        string? version,
        string? description,
        IReadOnlyList<ModuleDependencyMetadata> dependencies)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Module name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(typeName))
        {
            throw new ArgumentException("Module type name cannot be empty.", nameof(typeName));
        }

        Name = name;
        TypeName = typeName;
        Version = version;
        Description = description;
        Dependencies = Array.AsReadOnly((dependencies ?? throw new ArgumentNullException(nameof(dependencies))).ToArray());
    }

    public string Name { get; }

    public string TypeName { get; }

    public string? Version { get; }

    public string? Description { get; }

    public IReadOnlyList<ModuleDependencyMetadata> Dependencies { get; }
}
