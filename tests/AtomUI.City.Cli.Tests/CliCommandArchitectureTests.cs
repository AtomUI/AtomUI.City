using System.Text.Json;
using AtomUI.City.Cli;

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

    [Fact]
    public void EnvelopeDiagnosticsRejectExternalListMutation()
    {
        var envelope = CliEnvelope.Failed(
            "atomui city doctor",
            CliExitCodes.ArgumentError,
            CliDiagnostic.Error("AUCCLI0001", "Missing city root"),
            CliDiagnostic.Error("AUCCLI0002", "Unknown command"));
        var diagnostics = Assert.IsAssignableFrom<IList<CliDiagnostic>>(envelope.Diagnostics);

        Assert.Throws<NotSupportedException>(() => diagnostics[0] = CliDiagnostic.Error("AUCCLI9999", "Changed"));
        Assert.Equal("AUCCLI0001", envelope.Diagnostics[0].Code);
        Assert.Equal("AUCCLI0002", envelope.Diagnostics[1].Code);
    }

    [Fact]
    public void EnvelopeCopiesDictionaryDataSnapshot()
    {
        var data = new Dictionary<string, object?> { ["path"] = "source" };
        var envelope = CliEnvelope.Succeeded("atomui city inspect", data);

        data["path"] = "changed";
        data["extra"] = true;

        var envelopeData = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(envelope.Data);
        Assert.Equal("source", envelopeData["path"]);
        Assert.False(envelopeData.ContainsKey("extra"));
    }

    [Fact]
    public void EnvelopeDictionaryDataRejectsExternalMutation()
    {
        var envelope = CliEnvelope.Succeeded(
            "atomui city inspect",
            new Dictionary<string, object?> { ["path"] = "source" });

        var envelopeData = Assert.IsAssignableFrom<IDictionary<string, object?>>(envelope.Data);

        Assert.Throws<NotSupportedException>(() => envelopeData["path"] = "changed");
    }
}
