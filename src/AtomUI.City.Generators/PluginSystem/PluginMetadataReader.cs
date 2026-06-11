using Microsoft.CodeAnalysis;

namespace AtomUI.City.Generators.PluginSystem;

public static class PluginMetadataReader
{
    private const string ContributionManifestAttributeName = "AtomUI.City.PluginSystem.ContributionManifestAttribute";
    private const string PluginAttributeName = "AtomUI.City.PluginSystem.PluginAttribute";
    private const string PluginCapabilityAttributeName = "AtomUI.City.PluginSystem.PluginCapabilityAttribute";
    private const string PluginDependencyAttributeName = "AtomUI.City.PluginSystem.PluginDependencyAttribute";

    public static PluginMetadata? Read(Compilation compilation)
    {
        if (compilation is null)
        {
            throw new ArgumentNullException(nameof(compilation));
        }

        var attributes = compilation.Assembly.GetAttributes();
        var pluginAttribute = attributes.FirstOrDefault(attribute => string.Equals(GetAttributeTypeName(attribute), PluginAttributeName, StringComparison.Ordinal));

        if (pluginAttribute is null)
        {
            return null;
        }

        return ReadPluginMetadata(pluginAttribute, attributes);
    }

    private static PluginMetadata? ReadPluginMetadata(AttributeData pluginAttribute, IReadOnlyList<AttributeData> attributes)
    {
        if (pluginAttribute.ConstructorArguments.Length < 3)
        {
            return null;
        }

        var pluginId = pluginAttribute.ConstructorArguments[0].Value as string;
        var packageId = pluginAttribute.ConstructorArguments[1].Value as string;
        var version = pluginAttribute.ConstructorArguments[2].Value as string;

        if (string.IsNullOrWhiteSpace(pluginId) ||
            string.IsNullOrWhiteSpace(packageId) ||
            string.IsNullOrWhiteSpace(version))
        {
            return null;
        }

        var capabilities = attributes
            .Where(attribute => string.Equals(GetAttributeTypeName(attribute), PluginCapabilityAttributeName, StringComparison.Ordinal))
            .Select(ReadCapability)
            .Where(capability => capability is not null)
            .Cast<PluginCapabilityMetadata>()
            .ToArray();
        var contributions = attributes
            .Where(attribute => string.Equals(GetAttributeTypeName(attribute), ContributionManifestAttributeName, StringComparison.Ordinal))
            .Select(ReadContribution)
            .Where(contribution => contribution is not null)
            .Cast<PluginContributionManifestMetadata>()
            .ToArray();
        var dependencies = attributes
            .Where(attribute => string.Equals(GetAttributeTypeName(attribute), PluginDependencyAttributeName, StringComparison.Ordinal))
            .Select(ReadDependency)
            .Where(dependency => dependency is not null)
            .Cast<PluginDependencyMetadata>()
            .ToArray();

        return new PluginMetadata(
            schemaVersion: "1.0",
            pluginId: pluginId!,
            packageId: packageId!,
            version: version!,
            displayNameKey: ReadNamedString(pluginAttribute, "DisplayNameKey") ?? string.Empty,
            descriptionKey: ReadNamedString(pluginAttribute, "DescriptionKey"),
            publisher: ReadNamedString(pluginAttribute, "Publisher"),
            mainAssembly: ReadNamedString(pluginAttribute, "MainAssembly") ?? string.Empty,
            targetFramework: ReadNamedString(pluginAttribute, "TargetFramework") ?? string.Empty,
            pluginApiVersion: ReadNamedString(pluginAttribute, "PluginApiVersion") ?? string.Empty,
            minHostVersion: ReadNamedString(pluginAttribute, "MinHostVersion") ?? string.Empty,
            maxHostVersion: ReadNamedString(pluginAttribute, "MaxHostVersion"),
            unloadable: ReadNamedBoolean(pluginAttribute, "Unloadable", defaultValue: true),
            aotCompatible: ReadNamedBoolean(pluginAttribute, "AotCompatible", defaultValue: false),
            capabilities,
            contributions,
            dependencies);
    }

    private static PluginCapabilityMetadata? ReadCapability(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        var name = attribute.ConstructorArguments[0].Value as string;

        return string.IsNullOrWhiteSpace(name)
            ? null
            : new PluginCapabilityMetadata(name!, ReadNamedStringArray(attribute, "Scope"));
    }

    private static PluginContributionManifestMetadata? ReadContribution(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length < 2)
        {
            return null;
        }

        var type = attribute.ConstructorArguments[0].Value as string;
        var path = attribute.ConstructorArguments[1].Value as string;

        return string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(path)
            ? null
            : new PluginContributionManifestMetadata(
                type!,
                path!,
                ReadNamedBoolean(attribute, "Required", defaultValue: false));
    }

    private static PluginDependencyMetadata? ReadDependency(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        var pluginId = attribute.ConstructorArguments[0].Value as string;

        return string.IsNullOrWhiteSpace(pluginId)
            ? null
            : new PluginDependencyMetadata(pluginId!, ReadNamedString(attribute, "VersionRange"));
    }

    private static IReadOnlyList<string> ReadNamedStringArray(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (!string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                continue;
            }

            if (argument.Value.Kind != TypedConstantKind.Array)
            {
                return [];
            }

            return argument
                .Value
                .Values
                .Select(value => value.Value as string)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Cast<string>()
                .ToArray();
        }

        return [];
    }

    private static bool ReadNamedBoolean(AttributeData attribute, string name, bool defaultValue)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                return argument.Value.Value is true;
            }
        }

        return defaultValue;
    }

    private static string? ReadNamedString(AttributeData attribute, string name)
    {
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
        return attribute.AttributeClass is null
            ? null
            : attribute.AttributeClass.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }
}
