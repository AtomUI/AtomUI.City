using System.Text.Json;
using AtomUI.City.Cli;

namespace AtomUI.City.Cli.Tests;

internal sealed class CliTestHost : IDisposable
{
    public CliTestHost()
    {
        WorkingDirectory = Path.Combine(Path.GetTempPath(), "AtomUICityCliTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(WorkingDirectory);
    }

    public string WorkingDirectory { get; }

    public async ValueTask<CliTestRun> RunAsync(params string[] args)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var exitCode = await CliApplication.RunAsync(
            args,
            output,
            error,
            new CliExecutionEnvironment(WorkingDirectory));

        return new CliTestRun(exitCode, output.ToString(), error.ToString());
    }

    public void Dispose()
    {
        if (Directory.Exists(WorkingDirectory))
        {
            Directory.Delete(WorkingDirectory, recursive: true);
        }
    }
}

internal sealed record CliTestRun(int ExitCode, string Output, string Error)
{
    public JsonDocument ReadJson()
    {
        return JsonDocument.Parse(Output);
    }
}
