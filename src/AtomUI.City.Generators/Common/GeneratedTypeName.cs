namespace AtomUI.City.Generators.Common;

public sealed class GeneratedTypeName
{
    public GeneratedTypeName(string @namespace, string name)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            throw new ArgumentException("Namespace cannot be empty.", nameof(@namespace));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        Namespace = @namespace;
        Name = name;
    }

    public string Namespace { get; }

    public string Name { get; }

    public string FullName => $"{Namespace}.{Name}";
}
