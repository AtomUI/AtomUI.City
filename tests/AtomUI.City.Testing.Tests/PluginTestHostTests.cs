using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class PluginTestHostTests
{
    [Fact]
    public async Task InstallAsyncCreatesPluginRecordInTestDirectory()
    {
        await using var host = PluginTestHost
            .CreateBuilder()
            .UsePlugin("Sample.Plugin", "1.0.0")
            .Build();

        var record = await host.InstallAsync("Sample.Plugin");

        Assert.Equal("Sample.Plugin", record.Id);
        Assert.Equal("1.0.0", record.Version);
        Assert.Equal(PluginTestState.Installed, record.State);
        Assert.True(File.Exists(Path.Combine(record.InstallPath, "plugin.json")));
        Assert.StartsWith(host.Host.Directory.RootPath, record.InstallPath, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ActivateDeactivateAndUnloadUpdatePluginState()
    {
        await using var host = PluginTestHost
            .CreateBuilder()
            .UsePlugin("Sample.Plugin", "1.0.0")
            .Build();

        await host.InstallAsync("Sample.Plugin");

        Assert.Equal(PluginTestState.Active, (await host.ActivateAsync("Sample.Plugin")).State);
        Assert.Equal(PluginTestState.Inactive, (await host.DeactivateAsync("Sample.Plugin")).State);
        Assert.Equal(PluginTestState.Unloaded, (await host.UnloadAsync("Sample.Plugin")).State);
    }
}
