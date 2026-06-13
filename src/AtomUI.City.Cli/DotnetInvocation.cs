namespace AtomUI.City.Cli;

public sealed class DotnetInvocation
{
    private DotnetInvocation(IReadOnlyList<string> arguments)
    {
        Arguments = Array.AsReadOnly(arguments.ToArray());
    }

    public string Executable { get; } = "dotnet";

    public IReadOnlyList<string> Arguments { get; }

    internal static DotnetInvocation Create(string command, CliCommandLine commandLine)
    {
        var arguments = new List<string> { command };
        var project = commandLine.GetOptionValue("--project");
        if (!string.IsNullOrWhiteSpace(project))
        {
            arguments.Add(project);
        }

        var configuration = commandLine.GetOptionValue("--configuration");
        if (!string.IsNullOrWhiteSpace(configuration))
        {
            arguments.Add("--configuration");
            arguments.Add(configuration);
        }

        var framework = commandLine.GetOptionValue("--framework");
        if (!string.IsNullOrWhiteSpace(framework))
        {
            arguments.Add("--framework");
            arguments.Add(framework);
        }

        return new DotnetInvocation(arguments);
    }
}
