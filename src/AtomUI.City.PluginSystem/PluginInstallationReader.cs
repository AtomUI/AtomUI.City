using System.Text.Json;

namespace AtomUI.City.PluginSystem;

public static class PluginInstallationReader
{
    public static PluginInstallation Read(string installRecordPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installRecordPath);

        using var document = JsonDocument.Parse(File.ReadAllText(installRecordPath));
        var root = document.RootElement;

        return new PluginInstallation(
            ReadRequiredString(root, "pluginId"),
            ReadRequiredString(root, "packageId"),
            ReadRequiredString(root, "version"),
            ReadRequiredString(root, "rootPath"),
            ReadRequiredString(root, "manifestPath"));
    }

    private static string ReadRequiredString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }
}
