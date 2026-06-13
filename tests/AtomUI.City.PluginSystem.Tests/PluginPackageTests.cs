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
