namespace AtomUI.City.Cli.Tests;

public sealed class CliNewAppTests
{
    [Fact]
    public async Task NewAppDryRunEmitsPlanWithoutWritingFiles()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync(
            "city",
            "new",
            "app",
            "SalesClient",
            "--output",
            host.WorkingDirectory,
            "--dry-run",
            "--json");

        Assert.Equal(0, run.ExitCode);
        Assert.False(Directory.Exists(Path.Combine(host.WorkingDirectory, "src", "SalesClient")));
        using var json = run.ReadJson();
        var changes = json.RootElement.GetProperty("data").GetProperty("plan").GetProperty("changes");
        Assert.Contains(
            changes.EnumerateArray(),
            change => change.GetProperty("path").GetString() == "src/SalesClient/SalesClient.csproj");
    }

    [Fact]
    public async Task NewAppCreatesMinimalApplicationAndTestProject()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync(
            "city",
            "new",
            "app",
            "SalesClient",
            "--namespace",
            "Company.SalesClient",
            "--output",
            host.WorkingDirectory,
            "--json");

        Assert.Equal(0, run.ExitCode);
        Assert.True(File.Exists(Path.Combine(host.WorkingDirectory, "src", "SalesClient", "SalesClient.csproj")));
        Assert.True(File.Exists(Path.Combine(host.WorkingDirectory, "src", "SalesClient", "Program.cs")));
        Assert.True(File.Exists(Path.Combine(host.WorkingDirectory, "src", "SalesClient", "App.axaml")));
        Assert.True(File.Exists(Path.Combine(host.WorkingDirectory, "tests", "SalesClient.Tests", "FeatureTestMatrix.md")));
        Assert.True(File.Exists(Path.Combine(host.WorkingDirectory, "tests", "SalesClient.Tests", "ApplicationSmokeTests.cs")));
    }

    [Fact]
    public async Task NewAppRejectsFrameworkNamespace()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync(
            "city",
            "new",
            "app",
            "SalesClient",
            "--namespace",
            "AtomUI.City.SalesClient",
            "--output",
            host.WorkingDirectory,
            "--json");

        Assert.Equal(2, run.ExitCode);
        using var json = run.ReadJson();
        Assert.Equal("AUCCLI0102", json.RootElement.GetProperty("diagnostics")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task NewAppRejectsAotWithDynamicPlugins()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync(
            "city",
            "new",
            "app",
            "SalesClient",
            "--use-aot",
            "--use-dynamic-plugins",
            "--output",
            host.WorkingDirectory,
            "--json");

        Assert.Equal(2, run.ExitCode);
        using var json = run.ReadJson();
        Assert.Equal("AUCCLI0103", json.RootElement.GetProperty("diagnostics")[0].GetProperty("code").GetString());
    }
}
