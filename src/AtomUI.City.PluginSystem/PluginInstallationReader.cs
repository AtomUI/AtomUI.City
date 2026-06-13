using System.Text.Json;

namespace AtomUI.City.PluginSystem;

public static class PluginInstallationReader
{
    public static PluginInstallation Read(string installRecordPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installRecordPath);

        using var document = JsonDocument.Parse(File.ReadAllText(installRecordPath));
        var root = document.RootElement;

        var pluginId = ReadRequiredString(root, "pluginId");
        var packageId = ReadRequiredString(root, "packageId");
        var version = ReadRequiredString(root, "version");
        var rootPath = ReadRequiredString(root, "rootPath");
        var manifestPath = ReadRequiredString(root, "manifestPath");

        return new PluginInstallation(
            pluginId,
            packageId,
            version,
            rootPath,
            manifestPath);
    }

    private static string ReadRequiredString(JsonElement element, string propertyName)
    {
        var value = element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException($"Plugin install record property '{propertyName}' must be a non-empty string.");
        }

        return value;
    }
}
