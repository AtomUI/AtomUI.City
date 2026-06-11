namespace AtomUI.City.Cli;

internal sealed class CliCommandLine
{
    private static readonly HashSet<string> ValueOptions =
    [
        "--verbosity",
        "--working-directory",
        "--namespace",
        "--target-framework",
        "--output",
        "--configuration",
        "--framework",
        "--project",
        "--output-root",
        "--plugins-root",
    ];

    private readonly Dictionary<string, string?> _options;

    private CliCommandLine(
        IReadOnlyList<string> positionals,
        Dictionary<string, string?> options)
    {
        Positionals = positionals;
        _options = options;
    }

    public IReadOnlyList<string> Positionals { get; }

    public bool HasOption(string option)
    {
        return _options.ContainsKey(option);
    }

    public string? GetOptionValue(string option)
    {
        return _options.GetValueOrDefault(option);
    }

    public static CliCommandLine Parse(string[] args)
    {
        var positionals = new List<string>();
        var options = new Dictionary<string, string?>(StringComparer.Ordinal);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                positionals.Add(arg);
                continue;
            }

            if (ValueOptions.Contains(arg))
            {
                options[arg] = i + 1 < args.Length ? args[++i] : null;
            }
            else
            {
                options[arg] = "true";
            }
        }

        return new CliCommandLine(positionals, options);
    }
}
