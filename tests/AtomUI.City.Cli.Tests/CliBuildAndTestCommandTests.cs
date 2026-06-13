using System.Reflection;
using AtomUI.City.Cli;

namespace AtomUI.City.Cli.Tests;

public sealed class CliBuildAndTestCommandTests
{
    [Fact]
    public async Task BuildDryRunEmitsDotnetBuildInvocation()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync(
            "city",
            "build",
            "--configuration",
            "Release",
            "--project",
            "src/App/App.csproj",
            "--dry-run",
            "--json");

        Assert.Equal(0, run.ExitCode);
        using var json = run.ReadJson();
        var invocation = json.RootElement.GetProperty("data").GetProperty("invocation");
        Assert.Equal("dotnet", invocation.GetProperty("executable").GetString());
        Assert.Equal("build", invocation.GetProperty("arguments")[0].GetString());
        Assert.Contains(
            invocation.GetProperty("arguments").EnumerateArray(),
            argument => argument.GetString() == "Release");
    }

    [Fact]
    public async Task TestDryRunEmitsDotnetTestInvocation()
    {
        using var host = new CliTestHost();

        var run = await host.RunAsync(
            "city",
            "test",
            "--project",
            "tests/App.Tests/App.Tests.csproj",
            "--dry-run",
            "--json");

        Assert.Equal(0, run.ExitCode);
        using var json = run.ReadJson();
        var invocation = json.RootElement.GetProperty("data").GetProperty("invocation");
        Assert.Equal("dotnet", invocation.GetProperty("executable").GetString());
        Assert.Equal("test", invocation.GetProperty("arguments")[0].GetString());
    }

    [Fact]
    public void DotnetInvocationArgumentsRejectExternalMutation()
    {
        var constructor = typeof(DotnetInvocation).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            [typeof(IReadOnlyList<string>)]);
        var invocation = Assert.IsType<DotnetInvocation>(
            constructor?.Invoke(
                [
                    new[]
                    {
                        "build",
                        "src/App/App.csproj",
                        "--configuration",
                        "Release",
                    },
                ]));
        var arguments = Assert.IsAssignableFrom<IList<string>>(invocation.Arguments);

        Assert.Throws<NotSupportedException>(() => arguments[0] = "changed");
        Assert.Equal("build", invocation.Arguments[0]);
        Assert.Equal("src/App/App.csproj", invocation.Arguments[1]);
    }
}
