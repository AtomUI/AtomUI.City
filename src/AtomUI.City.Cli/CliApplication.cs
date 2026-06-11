using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using AtomUI.City.Templates;

namespace AtomUI.City.Cli;

public static class CliApplication
{
    public static async ValueTask<int> RunAsync(
        string[] args,
        TextWriter output,
        TextWriter error,
        CliExecutionEnvironment? environment = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        var commandLine = CliCommandLine.Parse(args);
        var executionEnvironment = environment ?? new CliExecutionEnvironment(Directory.GetCurrentDirectory());

        if (commandLine.Positionals.Count == 0 || commandLine.Positionals[0] != "city")
        {
            return await WriteAsync(
                    output,
                    commandLine,
                    "atomui city",
                    CliEnvelope.Failed(
                        "atomui city",
                        CliExitCodes.ArgumentError,
                        CliDiagnostic.Error("AUCCLI0001", "Command must start with 'city'.")))
                .ConfigureAwait(false);
        }

        return await DispatchAsync(commandLine, executionEnvironment, output, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<int> DispatchAsync(
        CliCommandLine commandLine,
        CliExecutionEnvironment environment,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        var positionals = commandLine.Positionals;
        var command = positionals.Count > 1 ? positionals[1] : "doctor";

        return command switch
        {
            "doctor" => await DoctorAsync(commandLine, environment, output).ConfigureAwait(false),
            "new" => await NewAsync(commandLine, environment, output).ConfigureAwait(false),
            "build" or "test" or "pack" or "publish" => await DotnetCommandAsync(command, commandLine, output, cancellationToken).ConfigureAwait(false),
            "inspect" => await InspectAsync(commandLine, environment, output).ConfigureAwait(false),
            "plugin" => await PluginAsync(commandLine, environment, output, cancellationToken).ConfigureAwait(false),
            "docs" when positionals.Count > 2 && positionals[2] == "check" => await GateCheckAsync("docs", commandLine, environment, output).ConfigureAwait(false),
            "tests" when positionals.Count > 2 && positionals[2] == "check" => await GateCheckAsync("tests", commandLine, environment, output).ConfigureAwait(false),
            "explain" => await ExplainAsync(commandLine, output).ConfigureAwait(false),
            "plan" => await GenericPlanAsync(commandLine, output).ConfigureAwait(false),
            "apply" => await ApplyAsync(commandLine, output).ConfigureAwait(false),
            "generate" => await GenericPlanAsync(commandLine, output).ConfigureAwait(false),
            _ => await WriteAsync(
                    output,
                    commandLine,
                    "atomui city " + command,
                    CliEnvelope.Failed(
                        "atomui city " + command,
                        CliExitCodes.ArgumentError,
                        CliDiagnostic.Error("AUCCLI0002", $"Unknown command '{command}'.")))
                .ConfigureAwait(false),
        };
    }

    private static async ValueTask<int> DoctorAsync(
        CliCommandLine commandLine,
        CliExecutionEnvironment environment,
        TextWriter output)
    {
        var solutionPath = Path.Combine(environment.WorkingDirectory, "AtomUICity.slnx");
        var data = new Dictionary<string, object?>
        {
            ["workingDirectory"] = environment.WorkingDirectory,
            ["solutionExists"] = File.Exists(solutionPath),
            ["docsDirectoryExists"] = Directory.Exists(Path.Combine(environment.WorkingDirectory, "docs")),
            ["testsDirectoryExists"] = Directory.Exists(Path.Combine(environment.WorkingDirectory, "tests")),
        };

        return await WriteAsync(
                output,
                commandLine,
                "atomui city doctor",
                CliEnvelope.Succeeded("atomui city doctor", data))
            .ConfigureAwait(false);
    }

    private static async ValueTask<int> NewAsync(
        CliCommandLine commandLine,
        CliExecutionEnvironment environment,
        TextWriter output)
    {
        var positionals = commandLine.Positionals;
        if (positionals.Count < 4 || positionals[2] != "app")
        {
            return await WriteAsync(
                    output,
                    commandLine,
                    "atomui city new app",
                    CliEnvelope.Failed(
                        "atomui city new app",
                        CliExitCodes.ArgumentError,
                        CliDiagnostic.Error("AUCCLI0101", "AppName is required.")))
                .ConfigureAwait(false);
        }

        var appName = positionals[3];
        var rootNamespace = commandLine.GetOptionValue("--namespace") ?? appName;
        if (rootNamespace.StartsWith("AtomUI.City", StringComparison.Ordinal))
        {
            return await WriteAsync(
                    output,
                    commandLine,
                    "atomui city new app",
                    CliEnvelope.Failed(
                        "atomui city new app",
                        CliExitCodes.ArgumentError,
                        CliDiagnostic.Error("AUCCLI0102", "Root namespace must not start with 'AtomUI.City'.")))
                .ConfigureAwait(false);
        }

        if (commandLine.HasOption("--use-aot") && commandLine.HasOption("--use-dynamic-plugins"))
        {
            return await WriteAsync(
                    output,
                    commandLine,
                    "atomui city new app",
                    CliEnvelope.Failed(
                        "atomui city new app",
                        CliExitCodes.ArgumentError,
                        CliDiagnostic.Error("AUCCLI0103", "--use-aot cannot be combined with --use-dynamic-plugins by default.")))
                .ConfigureAwait(false);
        }

        var outputPath = commandLine.GetOptionValue("--output") ?? environment.WorkingDirectory;
        var options = new ApplicationTemplateOptions
        {
            AppName = appName,
            RootNamespace = rootNamespace,
            OutputPath = Path.GetFullPath(outputPath, environment.WorkingDirectory),
            TargetFramework = commandLine.GetOptionValue("--target-framework") ?? "net10.0",
            IncludeTests = !commandLine.HasOption("--no-tests"),
            IncludeSample = commandLine.HasOption("--sample"),
            UseAot = commandLine.HasOption("--use-aot"),
            UseDynamicPlugins = commandLine.HasOption("--use-dynamic-plugins"),
        };
        var renderer = new ApplicationTemplateRenderer();
        var plan = renderer.CreatePlan(options);

        if (!commandLine.HasOption("--dry-run"))
        {
            renderer.Render(options);
        }

        return await WriteAsync(
                output,
                commandLine,
                "atomui city new app",
                CliEnvelope.Succeeded(
                    "atomui city new app",
                    new Dictionary<string, object?> { ["plan"] = plan }))
            .ConfigureAwait(false);
    }

    private static async ValueTask<int> DotnetCommandAsync(
        string command,
        CliCommandLine commandLine,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        var invocation = DotnetInvocation.Create(command, commandLine);

        if (!commandLine.HasOption("--dry-run"))
        {
            var result = await ProcessRunner.RunAsync(invocation, cancellationToken).ConfigureAwait(false);
            return await WriteAsync(
                    output,
                    commandLine,
                    "atomui city " + command,
                    result.ExitCode == 0
                        ? CliEnvelope.Succeeded("atomui city " + command, new Dictionary<string, object?> { ["invocation"] = invocation, ["exitCode"] = result.ExitCode })
                        : CliEnvelope.Failed("atomui city " + command, result.ExitCode, CliDiagnostic.Error("AUCCLI0201", result.Error)))
                .ConfigureAwait(false);
        }

        return await WriteAsync(
                output,
                commandLine,
                "atomui city " + command,
                CliEnvelope.Succeeded(
                    "atomui city " + command,
                    new Dictionary<string, object?> { ["invocation"] = invocation }))
            .ConfigureAwait(false);
    }

    private static async ValueTask<int> InspectAsync(
        CliCommandLine commandLine,
        CliExecutionEnvironment environment,
        TextWriter output)
    {
        var target = commandLine.Positionals.Count > 2 ? commandLine.Positionals[2] : "workspace";
        if (target != "workspace")
        {
            return await WriteAsync(
                    output,
                    commandLine,
                    "atomui city inspect " + target,
                    CliEnvelope.Succeeded("atomui city inspect " + target, new Dictionary<string, object?> { ["target"] = target }))
                .ConfigureAwait(false);
        }

        var solutionPath = Path.Combine(environment.WorkingDirectory, "AtomUICity.slnx");
        var projects = File.Exists(solutionPath)
            ? ReadSolutionProjects(solutionPath, environment.WorkingDirectory)
            : [];
        var data = new Dictionary<string, object?>
        {
            ["solution"] = File.Exists(solutionPath) ? "AtomUICity.slnx" : null,
            ["projects"] = projects,
            ["docsStatus"] = new { exists = Directory.Exists(Path.Combine(environment.WorkingDirectory, "docs")) },
            ["testMatrixStatus"] = new { exists = Directory.Exists(Path.Combine(environment.WorkingDirectory, "tests")) },
            ["buildOutputStatus"] = new { path = "output", exists = Directory.Exists(Path.Combine(environment.WorkingDirectory, "output")) },
        };

        return await WriteAsync(
                output,
                commandLine,
                "atomui city inspect workspace",
                CliEnvelope.Succeeded("atomui city inspect workspace", data))
            .ConfigureAwait(false);
    }

    private static async ValueTask<int> PluginAsync(
        CliCommandLine commandLine,
        CliExecutionEnvironment environment,
        TextWriter output,
        CancellationToken cancellationToken)
    {
        var action = commandLine.Positionals.Count > 2 ? commandLine.Positionals[2] : "list";
        var pluginsRoot = Path.GetFullPath(
            commandLine.GetOptionValue("--plugins-root") ?? Path.Combine(environment.WorkingDirectory, "plugins"),
            environment.WorkingDirectory);

        if (action == "list")
        {
            var plugins = ReadInstalledPlugins(pluginsRoot);
            return await WriteAsync(
                    output,
                    commandLine,
                    "atomui city plugin list",
                    CliEnvelope.Succeeded("atomui city plugin list", new Dictionary<string, object?> { ["plugins"] = plugins }))
                .ConfigureAwait(false);
        }

        if (action == "install")
        {
            var packagePath = commandLine.Positionals.Count > 3 ? commandLine.Positionals[3] : string.Empty;
            var plan = new
            {
                schemaVersion = "1.0",
                operationId = "plugin-install",
                command = "atomui city plugin install",
                changes = new[]
                {
                    new { type = "install-plugin", path = packagePath, pluginsRoot },
                },
            };

            if (!commandLine.HasOption("--dry-run") && string.IsNullOrWhiteSpace(packagePath))
            {
                return await WriteAsync(
                        output,
                        commandLine,
                        "atomui city plugin install",
                        CliEnvelope.Failed(
                            "atomui city plugin install",
                            CliExitCodes.ArgumentError,
                            CliDiagnostic.Error("AUCCLI0301", "Plugin package path is required.")))
                    .ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return await WriteAsync(
                    output,
                    commandLine,
                    "atomui city plugin install",
                    CliEnvelope.Succeeded("atomui city plugin install", new Dictionary<string, object?> { ["plan"] = plan }))
                .ConfigureAwait(false);
        }

        return await WriteAsync(
                output,
                commandLine,
                "atomui city plugin " + action,
                CliEnvelope.Succeeded(
                    "atomui city plugin " + action,
                    new Dictionary<string, object?> { ["action"] = action, ["pluginsRoot"] = pluginsRoot }))
            .ConfigureAwait(false);
    }

    private static async ValueTask<int> GateCheckAsync(
        string gate,
        CliCommandLine commandLine,
        CliExecutionEnvironment environment,
        TextWriter output)
    {
        var path = Path.Combine(environment.WorkingDirectory, gate == "docs" ? "docs" : "tests");
        return await WriteAsync(
                output,
                commandLine,
                $"atomui city {gate} check",
                CliEnvelope.Succeeded(
                    $"atomui city {gate} check",
                    new Dictionary<string, object?> { ["path"] = path, ["exists"] = Directory.Exists(path) }))
            .ConfigureAwait(false);
    }

    private static async ValueTask<int> ExplainAsync(CliCommandLine commandLine, TextWriter output)
    {
        var code = commandLine.Positionals.Count > 2 ? commandLine.Positionals[2] : "AUCCLI0000";
        var data = new Dictionary<string, object?>
        {
            ["code"] = code,
            ["reason"] = "See AtomUI.City CLI diagnostics documentation.",
            ["suggestedAction"] = "Run the command with --json --pretty for structured diagnostics.",
        };

        return await WriteAsync(output, commandLine, "atomui city explain", CliEnvelope.Succeeded("atomui city explain", data)).ConfigureAwait(false);
    }

    private static async ValueTask<int> GenericPlanAsync(CliCommandLine commandLine, TextWriter output)
    {
        var command = "atomui " + string.Join(' ', commandLine.Positionals);
        var data = new Dictionary<string, object?>
        {
            ["plan"] = new
            {
                schemaVersion = "1.0",
                operationId = "cli-plan",
                command,
                changes = Array.Empty<object>(),
            },
        };

        return await WriteAsync(output, commandLine, command, CliEnvelope.Succeeded(command, data)).ConfigureAwait(false);
    }

    private static async ValueTask<int> ApplyAsync(CliCommandLine commandLine, TextWriter output)
    {
        var command = "atomui city apply";
        var planFile = commandLine.Positionals.Count > 2 ? commandLine.Positionals[2] : string.Empty;
        var data = new Dictionary<string, object?> { ["planFile"] = planFile };

        return await WriteAsync(output, commandLine, command, CliEnvelope.Succeeded(command, data)).ConfigureAwait(false);
    }

    private static IReadOnlyList<object> ReadSolutionProjects(string solutionPath, string workingDirectory)
    {
        try
        {
            var document = XDocument.Load(solutionPath);
            return document
                .Descendants("Project")
                .Select(project => project.Attribute("Path")?.Value)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => new
                {
                    path,
                    exists = File.Exists(Path.Combine(workingDirectory, path!)),
                })
                .Cast<object>()
                .ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static IReadOnlyList<object> ReadInstalledPlugins(string pluginsRoot)
    {
        var installedRoot = Path.Combine(pluginsRoot, "installed");
        if (!Directory.Exists(installedRoot))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(installedRoot, "plugin.json", SearchOption.AllDirectories)
            .Select(ReadPluginManifest)
            .Where(plugin => plugin is not null)
            .Cast<object>()
            .ToArray();
    }

    private static object? ReadPluginManifest(string manifestPath)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
            var root = document.RootElement;

            return new
            {
                pluginId = root.GetProperty("pluginId").GetString(),
                packageId = root.GetProperty("packageId").GetString(),
                version = root.GetProperty("version").GetString(),
                manifestPath,
            };
        }
        catch
        {
            return null;
        }
    }

    private static async ValueTask<int> WriteAsync(
        TextWriter output,
        CliCommandLine commandLine,
        string command,
        CliEnvelope envelope)
    {
        if (commandLine.HasOption("--json"))
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = commandLine.HasOption("--pretty"),
            };

            await output.WriteLineAsync(JsonSerializer.Serialize(envelope, options)).ConfigureAwait(false);
        }
        else
        {
            await output.WriteLineAsync(envelope.Success ? $"{command}: OK" : $"{command}: failed").ConfigureAwait(false);
        }

        return envelope.ExitCode;
    }
}
