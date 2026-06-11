using Microsoft.CodeAnalysis;

namespace AtomUI.City.Generators.Modularity;

public static class ModuleMetadataReader
{
    private const string DependsOnAttributeName = "AtomUI.City.Modularity.DependsOnAttribute";
    private const string ModuleAttributeName = "AtomUI.City.Modularity.ModuleAttribute";
    private const string ModuleBaseName = "AtomUI.City.Modularity.ModuleBase";
    private const string ModuleInterfaceName = "AtomUI.City.Modularity.IModule";

    public static ModuleMetadata? TryRead(INamedTypeSymbol type)
    {
        if (!IsModule(type))
        {
            return null;
        }

        var typeName = GetTypeName(type);
        var moduleAttribute = type
            .GetAttributes()
            .FirstOrDefault(attribute => string.Equals(GetAttributeTypeName(attribute), ModuleAttributeName, StringComparison.Ordinal));
        var name = ReadModuleName(moduleAttribute) ?? typeName;
        var version = ReadNamedString(moduleAttribute, "Version");
        var description = ReadNamedString(moduleAttribute, "Description");
        var dependencies = type
            .GetAttributes()
            .Where(attribute => string.Equals(GetAttributeTypeName(attribute), DependsOnAttributeName, StringComparison.Ordinal))
            .Select(ReadDependency)
            .Where(dependency => dependency is not null)
            .Cast<ModuleDependencyMetadata>()
            .ToArray();

        return new ModuleMetadata(name, typeName, version, description, dependencies);
    }

    private static bool IsModule(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (string.Equals(GetTypeName(current), ModuleBaseName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return type
            .AllInterfaces
            .Any(@interface => string.Equals(GetTypeName(@interface), ModuleInterfaceName, StringComparison.Ordinal));
    }

    private static ModuleDependencyMetadata? ReadDependency(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        var dependencyType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;

        if (dependencyType is null)
        {
            return null;
        }

        return new ModuleDependencyMetadata(
            GetTypeName(dependencyType),
            ReadNamedBoolean(attribute, "Optional"));
    }

    private static string? ReadModuleName(AttributeData? attribute)
    {
        if (attribute is null || attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        return attribute.ConstructorArguments[0].Value as string;
    }

    private static bool ReadNamedBoolean(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                return argument.Value.Value is true;
            }
        }

        return false;
    }

    private static string? ReadNamedString(AttributeData? attribute, string name)
    {
        if (attribute is null)
        {
            return null;
        }

        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                return argument.Value.Value as string;
            }
        }

        return null;
    }

    private static string? GetAttributeTypeName(AttributeData attribute)
    {
        return attribute.AttributeClass is null ? null : GetTypeName(attribute.AttributeClass);
    }

    private static string GetTypeName(INamedTypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }
}
