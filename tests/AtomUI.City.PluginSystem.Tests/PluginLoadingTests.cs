using AtomUI.City.PluginSystem;

namespace AtomUI.City.PluginSystem.Tests;

public sealed class PluginLoadingTests
{
    [Fact]
    public async Task LoaderLoadsMainAssemblyFromPluginRoot()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest(mainAssembly: "AtomUI.City.PluginSystem.dll");
        workspace.CopyMainAssembly("AtomUI.City.PluginSystem.dll");
        var descriptor = PluginDescriptor.FromManifest(
            PluginManifestReader.Read(workspace.ManifestPath),
            workspace.Root);
        var loader = new PluginLoader();

        var result = await loader.LoadAsync(descriptor);

        Assert.True(result.Succeeded);
        Assert.Equal(PluginRuntimeState.Loaded, result.Runtime.State);
        Assert.Equal("AtomUI.City.PluginSystem", result.Runtime.MainAssembly.GetName().Name);
    }

    [Fact]
    public async Task RuntimeDeactivateAndUnloadUpdateState()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest(mainAssembly: "AtomUI.City.PluginSystem.dll");
        workspace.CopyMainAssembly("AtomUI.City.PluginSystem.dll");
        var descriptor = PluginDescriptor.FromManifest(
            PluginManifestReader.Read(workspace.ManifestPath),
            workspace.Root);
        var loader = new PluginLoader();
        var result = await loader.LoadAsync(descriptor);

        result.Runtime.Activate();
        await result.Runtime.DeactivateAsync();
        await result.Runtime.UnloadAsync();

        Assert.Equal(PluginRuntimeState.Unloaded, result.Runtime.State);
    }

    [Fact]
    public async Task LoaderRejectsMissingMainAssembly()
    {
        using var workspace = new PluginTestWorkspace();
        workspace.WriteStandardManifest(mainAssembly: "Missing.Plugin.dll");
        var descriptor = PluginDescriptor.FromManifest(
            PluginManifestReader.Read(workspace.ManifestPath),
            workspace.Root);
        var loader = new PluginLoader();

        var result = await loader.LoadAsync(descriptor);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == PluginDiagnosticIds.MainAssemblyNotFound);
    }
}
