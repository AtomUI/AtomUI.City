using System.Text.Json;

namespace AtomUI.City.PluginSystem;

public static class PluginManifestReader
{
    public static PluginManifest Read(string manifestPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;

        return new PluginManifest(
            ReadRequiredString(root, "schemaVersion"),
            ReadRequiredString(root, "pluginId"),
            ReadRequiredString(root, "packageId"),
            ReadRequiredString(root, "version"),
            ReadRequiredString(root, "displayNameKey"),
            ReadOptionalString(root, "descriptionKey"),
            ReadOptionalString(root, "publisher"),
            ReadRequiredString(root, "mainAssembly"),
            ReadRequiredString(root, "targetFramework"),
            ReadRequiredString(root, "pluginApiVersion"),
            ReadRequiredString(root, "minHostVersion"),
            ReadOptionalString(root, "maxHostVersion"),
            ReadBoolean(root, "unloadable", defaultValue: true),
            ReadBoolean(root, "aotCompatible", defaultValue: false),
            ReadCapabilities(root),
            ReadContributions(root),
            ReadDependencies(root),
            ReadModules(root));
    }

    private static IReadOnlyList<PluginCapabilityDescriptor> ReadCapabilities(JsonElement root)
    {
        if (!root.TryGetProperty("capabilities", out var capabilities) ||
            capabilities.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return capabilities
            .EnumerateArray()
            .Select(capability => new PluginCapabilityDescriptor(
                ReadRequiredString(capability, "name"),
                ReadStringArray(capability, "scope")))
            .ToArray();
    }

    private static IReadOnlyList<PluginContributionDescriptor> ReadContributions(JsonElement root)
    {
        if (!root.TryGetProperty("contributions", out var contributions))
        {
            return [];
        }

        if (contributions.ValueKind == JsonValueKind.Object)
        {
            return contributions
                .EnumerateObject()
                .Select(property => new PluginContributionDescriptor(
                    property.Name,
                    ReadRequiredString(property.Value, "path"),
                    ReadBoolean(property.Value, "required", defaultValue: false)))
                .ToArray();
        }

        if (contributions.ValueKind == JsonValueKind.Array)
        {
            return contributions
                .EnumerateArray()
                .Select(contribution => new PluginContributionDescriptor(
                    ReadRequiredString(contribution, "type"),
                    ReadRequiredString(contribution, "path"),
                    ReadBoolean(contribution, "required", defaultValue: false)))
                .ToArray();
        }

        return [];
    }

    private static IReadOnlyList<PluginDependencyDescriptor> ReadDependencies(JsonElement root)
    {
        if (!root.TryGetProperty("dependencies", out var dependencies))
        {
            return [];
        }

        if (dependencies.ValueKind == JsonValueKind.Object &&
            dependencies.TryGetProperty("plugins", out var plugins) &&
            plugins.ValueKind == JsonValueKind.Array)
        {
            return plugins
                .EnumerateArray()
                .Select(dependency => new PluginDependencyDescriptor(
                    ReadRequiredString(dependency, "pluginId"),
                    ReadOptionalString(dependency, "versionRange")))
                .ToArray();
        }

        if (dependencies.ValueKind == JsonValueKind.Array)
        {
            return dependencies
                .EnumerateArray()
                .Select(dependency => new PluginDependencyDescriptor(
                    ReadRequiredString(dependency, "pluginId"),
                    ReadOptionalString(dependency, "versionRange")))
                .ToArray();
        }

        return [];
    }

    private static IReadOnlyList<PluginModuleDescriptor> ReadModules(JsonElement root)
    {
        if (!root.TryGetProperty("modules", out var modules) ||
            modules.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return modules
            .EnumerateArray()
            .Select(module => new PluginModuleDescriptor(
                ReadRequiredString(module, "name"),
                ReadRequiredString(module, "type"),
                ReadStringArray(module, "dependencies")))
            .ToArray();
    }

    private static string ReadRequiredString(JsonElement element, string propertyName)
    {
        return ReadOptionalString(element, propertyName) ?? string.Empty;
    }

    private static string? ReadOptionalString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static bool ReadBoolean(JsonElement element, string propertyName, bool defaultValue)
    {
        return element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : defaultValue;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return property
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }
}
