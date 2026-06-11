using System.Text.Json;

namespace AtomUI.City.Cli.Tests;

public sealed class CliCommandArchitectureTests
{
    [Fact]
    public async Task DoctorCommandWritesJsonEnvelope()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync("city", "doctor", "--json");

        Assert.Equal(0, run.ExitCode);
        using var json = run.ReadJson();
        var root = json.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("atomui city doctor", root.GetProperty("command").GetString());
        Assert.True(root.GetProperty("success").GetBoolean());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("diagnostics").ValueKind);
        Assert.Equal(JsonValueKind.Object, root.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task MissingCityRootReturnsStableDiagnostic()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync("doctor", "--json");

        Assert.Equal(2, run.ExitCode);
        using var json = run.ReadJson();
        var root = json.RootElement;
        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal("AUCCLI0001", root.GetProperty("diagnostics")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task UnknownCommandReturnsStableDiagnostic()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync("city", "unknown", "--json");

        Assert.Equal(2, run.ExitCode);
        using var json = run.ReadJson();
        Assert.Equal("AUCCLI0002", json.RootElement.GetProperty("diagnostics")[0].GetProperty("code").GetString());
    }
}
