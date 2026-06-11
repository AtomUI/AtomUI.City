namespace AtomUI.City.Cli;

public sealed class CliExecutionEnvironment
{
    public CliExecutionEnvironment(string workingDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        WorkingDirectory = workingDirectory;
    }

    public string WorkingDirectory { get; }
}
