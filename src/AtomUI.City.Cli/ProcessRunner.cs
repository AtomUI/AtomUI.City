using System.Diagnostics;

namespace AtomUI.City.Cli;

internal static class ProcessRunner
{
    public static async ValueTask<ProcessRunResult> RunAsync(
        DotnetInvocation invocation,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = invocation.Executable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        foreach (var argument in invocation.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return new ProcessRunResult(CliExitCodes.Failure, string.Empty, "Failed to start dotnet process.");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return new ProcessRunResult(
            process.ExitCode,
            await outputTask.ConfigureAwait(false),
            await errorTask.ConfigureAwait(false));
    }
}

internal sealed record ProcessRunResult(int ExitCode, string Output, string Error);
