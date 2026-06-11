using AtomUI.City.PluginSystem;

namespace AtomUI.City.PluginSystem.Tests;

internal sealed class PluginTestWorkspace : IDisposable
{
    public PluginTestWorkspace()
    {
        Temp = Path.Combine(Path.GetTempPath(), "AtomUICityPluginTests", Guid.NewGuid().ToString("N"));
        Root = Path.Combine(Temp, "package");
        ManifestPath = Path.Combine(Root, PluginPackagePaths.ManifestRelativePath);
        Directory.CreateDirectory(Root);
    }

    public string Temp { get; }

    public string Root { get; }

    public string ManifestPath { get; }

    public string CreateDirectory(params string[] segments)
    {
        var path = Path.Combine([Temp, .. segments]);
        Directory.CreateDirectory(path);

        return path;
    }

    public string WriteManifest(string json)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ManifestPath)!);
        File.WriteAllText(ManifestPath, json);

        return ManifestPath;
    }

    public string WriteStandardManifest(
        string mainAssembly = "Company.Sales.Plugin.dll",
        string? requiredContributionPath = null)
    {
        var contribution = requiredContributionPath is null
            ? string.Empty
            : $$"""
              ,
              "contributions": {
                "routes": { "path": "{{requiredContributionPath}}", "required": true }
              }
              """;

        return WriteManifest(
            $$"""
            {
              "schemaVersion": "1.0",
              "pluginId": "com.company.sales",
              "packageId": "Company.Sales.Plugin",
              "version": "1.0.0",
              "displayNameKey": "SalesPlugin.DisplayName",
              "mainAssembly": "{{mainAssembly}}",
              "targetFramework": "net10.0",
              "pluginApiVersion": "1.0",
              "minHostVersion": "1.0.0",
              "unloadable": true,
              "aotCompatible": false
              {{contribution}}
            }
            """);
    }

    public string CopyMainAssembly(string fileName)
    {
        var assemblyPath = typeof(PluginAttribute).Assembly.Location;
        var targetDirectory = Path.Combine(Root, "lib", "net10.0");
        var targetPath = Path.Combine(targetDirectory, fileName);
        Directory.CreateDirectory(targetDirectory);
        File.Copy(assemblyPath, targetPath, overwrite: true);

        return targetPath;
    }

    public void Dispose()
    {
        if (Directory.Exists(Temp))
        {
            Directory.Delete(Temp, recursive: true);
        }
    }
}
