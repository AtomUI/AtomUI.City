using System.IO.Compression;
using AtomUI.City.PluginSystem;

namespace AtomUI.City.PluginSystem.Tests;

public sealed class PluginPackageTests
{
    [Fact]
    public void PackageLayoutValidatorRequiresPluginManifest()
    {
        using var workspace = new PluginTestWorkspace();

        var result = PluginPackageLayoutValidator.Validate(workspace.Root);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.ManifestNotFound);
    }

    [Fact]
    public void PackageLayoutValidatorRequiresRequiredContributionManifests()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest(requiredContributionPath: "atomui-city/manifests/routes.json");
        workspace.CopyMainAssembly("Company.Sales.Plugin.dll");

        var result = PluginPackageLayoutValidator.Validate(workspace.Root);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.RequiredContributionManifestNotFound);
    }

    [Fact]
    public void PackageLayoutValidatorDoesNotProbeInvalidContributionPaths()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest(requiredContributionPath: "../routes.json");
        workspace.CopyMainAssembly("Company.Sales.Plugin.dll");

        var result = PluginPackageLayoutValidator.Validate(workspace.Root);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidContributionPath);
        Assert.DoesNotContain(
            result.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.RequiredContributionManifestNotFound);
    }

    [Fact]
    public void PackageLayoutValidatorDoesNotProbeInvalidTargetFrameworkPaths()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteManifest(
            """
            {
              "schemaVersion": "1.0",
              "pluginId": "com.company.sales",
              "packageId": "Company.Sales.Plugin",
              "version": "1.0.0",
              "displayNameKey": "SalesPlugin.DisplayName",
              "mainAssembly": "Company.Sales.Plugin.dll",
              "targetFramework": "../net10.0",
              "pluginApiVersion": "1.0",
              "minHostVersion": "1.0.0",
              "unloadable": true,
              "aotCompatible": false
            }
            """);

        var result = PluginPackageLayoutValidator.Validate(workspace.Root);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidTargetFramework);
        Assert.DoesNotContain(
            result.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.MainAssemblyNotFound);
    }

    [Fact]
    public async Task InstallerInstallsPackageIntoVersionedRuntimeRoot()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest();
        workspace.CopyMainAssembly("Company.Sales.Plugin.dll");
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installer = new PluginPackageInstaller();

        var result = await installer.InstallFromDirectoryAsync(workspace.Root, pluginsRoot);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Installation);
        Assert.Equal(
            Path.Combine(pluginsRoot, "installed", "com.company.sales", "1.0.0", "root"),
            result.Installation.RootPath);
        Assert.True(File.Exists(result.Installation.ManifestPath));
        Assert.True(File.Exists(Path.Combine(pluginsRoot, "installed", "com.company.sales", "1.0.0", "install.json")));
    }

    [Fact]
    public async Task InstallationReaderReadsInstallRecord()
    {
        using var workspace = new PluginTestWorkspace();
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installedVersionPath = Path.Combine(pluginsRoot, "installed", "com.company.sales", "1.0.0");
        var installedRootPath = Path.Combine(installedVersionPath, "root");
        var manifestPath = Path.Combine(installedRootPath, "atomui-city", "plugin.json");
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        var installRecordPath = Path.Combine(installedVersionPath, "install.json");
        await File.WriteAllTextAsync(
            installRecordPath,
            $$"""
            {
              "pluginId": "com.company.sales",
              "packageId": "Company.Sales.Plugin",
              "version": "1.0.0",
              "rootPath": "{{installedRootPath}}",
              "manifestPath": "{{manifestPath}}"
            }
            """);

        var installation = PluginInstallationReader.Read(installRecordPath);

        Assert.Equal("com.company.sales", installation.PluginId);
        Assert.Equal("Company.Sales.Plugin", installation.PackageId);
        Assert.Equal("1.0.0", installation.Version);
        Assert.Equal(installedRootPath, installation.RootPath);
        Assert.Equal(manifestPath, installation.ManifestPath);
    }

    [Fact]
    public async Task DiscoveryScannerFindsInstalledPlugins()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest();
        workspace.CopyMainAssembly("Company.Sales.Plugin.dll");
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installer = new PluginPackageInstaller();
        var installResult = await installer.InstallFromDirectoryAsync(workspace.Root, pluginsRoot);
        Assert.NotNull(installResult.Installation);

        var discovery = PluginDiscoveryScanner.DiscoverInstalled(pluginsRoot);

        Assert.True(discovery.Succeeded);
        var plugin = Assert.Single(discovery.Plugins);
        Assert.Equal("com.company.sales", plugin.PluginId);
        Assert.Equal("1.0.0", plugin.Version);
        Assert.Equal(installResult.Installation.RootPath, plugin.RootPath);
    }

    [Fact]
    public async Task DiscoveryScannerReportsMissingInstalledManifest()
    {
        using var workspace = new PluginTestWorkspace();
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installedVersionPath = Path.Combine(pluginsRoot, "installed", "com.company.sales", "1.0.0");
        var installedRootPath = Path.Combine(installedVersionPath, "root");
        var manifestPath = Path.Combine(installedRootPath, "atomui-city", "plugin.json");
        Directory.CreateDirectory(installedVersionPath);
        await File.WriteAllTextAsync(
            Path.Combine(installedVersionPath, "install.json"),
            $$"""
            {
              "pluginId": "com.company.sales",
              "packageId": "Company.Sales.Plugin",
              "version": "1.0.0",
              "rootPath": "{{installedRootPath}}",
              "manifestPath": "{{manifestPath}}"
            }
            """);

        var discovery = PluginDiscoveryScanner.DiscoverInstalled(pluginsRoot);

        Assert.False(discovery.Succeeded);
        Assert.Empty(discovery.Plugins);
        Assert.Contains(
            discovery.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.ManifestNotFound
                && diagnostic.PluginId == "com.company.sales"
                && diagnostic.Path == manifestPath);
    }

    [Fact]
    public async Task DiscoveryScannerReportsInvalidInstallRecordsAndContinues()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest();
        workspace.CopyMainAssembly("Company.Sales.Plugin.dll");
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installer = new PluginPackageInstaller();
        await installer.InstallFromDirectoryAsync(workspace.Root, pluginsRoot);
        var invalidRecordPath = Path.Combine(pluginsRoot, "installed", "com.company.broken", "1.0.0", "install.json");
        Directory.CreateDirectory(Path.GetDirectoryName(invalidRecordPath)!);
        await File.WriteAllTextAsync(invalidRecordPath, "not json");

        var discovery = PluginDiscoveryScanner.DiscoverInstalled(pluginsRoot);

        Assert.False(discovery.Succeeded);
        Assert.Single(discovery.Plugins);
        Assert.Contains(
            discovery.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidInstallRecord
                && diagnostic.Field == "installRecord"
                && diagnostic.Path == invalidRecordPath);
    }

    [Fact]
    public async Task DiscoveryScannerReportsInstallRecordVersionMismatch()
    {
        using var workspace = new PluginTestWorkspace();
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installedVersionPath = Path.Combine(pluginsRoot, "installed", "com.company.sales", "1.0.0");
        var installedRootPath = Path.Combine(installedVersionPath, "root");
        var manifestPath = Path.Combine(installedRootPath, "atomui-city", "plugin.json");
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        await File.WriteAllTextAsync(
            manifestPath,
            """
            {
              "schemaVersion": "1.0",
              "pluginId": "com.company.sales",
              "packageId": "Company.Sales.Plugin",
              "version": "2.0.0",
              "displayNameKey": "SalesPlugin.DisplayName",
              "mainAssembly": "Company.Sales.Plugin.dll",
              "targetFramework": "net10.0",
              "pluginApiVersion": "1.0",
              "minHostVersion": "1.0.0",
              "unloadable": true,
              "aotCompatible": false
            }
            """);
        await File.WriteAllTextAsync(
            Path.Combine(installedVersionPath, "install.json"),
            $$"""
            {
              "pluginId": "com.company.sales",
              "packageId": "Company.Sales.Plugin",
              "version": "1.0.0",
              "rootPath": "{{installedRootPath}}",
              "manifestPath": "{{manifestPath}}"
            }
            """);

        var discovery = PluginDiscoveryScanner.DiscoverInstalled(pluginsRoot);

        Assert.False(discovery.Succeeded);
        Assert.Empty(discovery.Plugins);
        Assert.Contains(
            discovery.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.PluginVersionMismatch
                && diagnostic.PluginId == "com.company.sales"
                && diagnostic.Field == "version");
    }

    [Fact]
    public async Task DiscoveryScannerReportsInvalidManifestJsonAndContinues()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest();
        workspace.CopyMainAssembly("Company.Sales.Plugin.dll");
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installer = new PluginPackageInstaller();
        await installer.InstallFromDirectoryAsync(workspace.Root, pluginsRoot);

        var installedVersionPath = Path.Combine(pluginsRoot, "installed", "com.company.broken", "1.0.0");
        var installedRootPath = Path.Combine(installedVersionPath, "root");
        var manifestPath = Path.Combine(installedRootPath, "atomui-city", "plugin.json");
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        await File.WriteAllTextAsync(manifestPath, "not json");
        await File.WriteAllTextAsync(
            Path.Combine(installedVersionPath, "install.json"),
            $$"""
            {
              "pluginId": "com.company.broken",
              "packageId": "Company.Broken.Plugin",
              "version": "1.0.0",
              "rootPath": "{{installedRootPath}}",
              "manifestPath": "{{manifestPath}}"
            }
            """);

        var discovery = PluginDiscoveryScanner.DiscoverInstalled(pluginsRoot);

        Assert.False(discovery.Succeeded);
        Assert.Single(discovery.Plugins);
        Assert.Contains(
            discovery.Diagnostics,
            diagnostic => diagnostic.Code == PluginDiagnosticIds.InvalidManifest
                && diagnostic.PluginId == "com.company.broken"
                && diagnostic.Field == "manifest"
                && diagnostic.Path == manifestPath);
    }

    [Fact]
    public async Task InstallerCanInstallFromNuGetPackageArchive()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest();
        workspace.CopyMainAssembly("Company.Sales.Plugin.dll");
        var packagePath = Path.Combine(workspace.Temp, "Company.Sales.Plugin.1.0.0.nupkg");
        ZipFile.CreateFromDirectory(workspace.Root, packagePath);
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installer = new PluginPackageInstaller();

        var result = await installer.InstallFromPackageAsync(packagePath, pluginsRoot);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Installation);
        Assert.True(Directory.Exists(result.Installation.RootPath));
        Assert.Equal("Company.Sales.Plugin", result.Installation.PackageId);
    }

    [Fact]
    public async Task InstallerDeletesStagingWhenPackageExtractionFails()
    {
        using var workspace = new PluginTestWorkspace();
        var packagePath = Path.Combine(workspace.Temp, "broken.nupkg");
        await File.WriteAllTextAsync(packagePath, "not a zip package");
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installer = new PluginPackageInstaller();

        var result = await installer.InstallFromPackageAsync(packagePath, pluginsRoot);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.PackageExtractionFailed);
        Assert.False(Directory.Exists(Path.Combine(pluginsRoot, PluginPackagePaths.StagingDirectoryName)));
    }

    [Fact]
    public async Task InstallerDeletesStagingWhenDirectoryInstallIsCancelled()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest();
        workspace.CopyMainAssembly("Company.Sales.Plugin.dll");
        var pluginsRoot = workspace.CreateDirectory("plugins");
        var installer = new PluginPackageInstaller();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => installer.InstallFromDirectoryAsync(workspace.Root, pluginsRoot, cancellation.Token).AsTask());

        Assert.False(Directory.Exists(Path.Combine(pluginsRoot, PluginPackagePaths.StagingDirectoryName)));
    }
}
