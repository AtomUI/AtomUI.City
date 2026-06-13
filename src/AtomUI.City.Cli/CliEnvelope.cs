namespace AtomUI.City.Cli;

public sealed class CliEnvelope
{
    private CliEnvelope(
        string command,
        bool success,
        int exitCode,
        IReadOnlyList<CliDiagnostic> diagnostics,
        object? data)
    {
        Command = command;
        Success = success;
        ExitCode = exitCode;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        Data = NormalizeData(data);
    }

    public string SchemaVersion { get; } = "1.0";

    public string Command { get; }

    public bool Success { get; }

    public int ExitCode { get; }

    public IReadOnlyList<CliDiagnostic> Diagnostics { get; }

    public object Data { get; }

    public IReadOnlyList<string> SuggestedActions { get; } = [];

    public IReadOnlyList<string> DocumentationLinks { get; } = [];

    public static CliEnvelope Succeeded(string command, object? data)
    {
        return new CliEnvelope(command, success: true, CliExitCodes.Success, [], data);
    }

    public static CliEnvelope Failed(
        string command,
        int exitCode,
        params CliDiagnostic[] diagnostics)
    {
        return new CliEnvelope(command, success: false, exitCode, diagnostics, data: new Dictionary<string, object?>());
    }

    private static object NormalizeData(object? data)
    {
        if (data is null)
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, object?>(
                new Dictionary<string, object?>(StringComparer.Ordinal));
        }

        return NormalizeValue(data) ?? new System.Collections.ObjectModel.ReadOnlyDictionary<string, object?>(
            new Dictionary<string, object?>(StringComparer.Ordinal));
    }

    private static object? NormalizeValue(object? value)
    {
        if (value is null or string)
        {
            return value;
        }

        if (value is IReadOnlyDictionary<string, object?> dictionary)
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, object?>(
                dictionary.ToDictionary(
                    pair => pair.Key,
                    pair => NormalizeValue(pair.Value),
                    StringComparer.Ordinal));
        }

        if (value is IReadOnlyList<object?> list)
        {
            return Array.AsReadOnly(list.Select(NormalizeValue).ToArray());
        }

        if (value is System.Collections.IList nonGenericList)
        {
            var normalized = new object?[nonGenericList.Count];
            for (var i = 0; i < nonGenericList.Count; i++)
            {
                normalized[i] = NormalizeValue(nonGenericList[i]);
            }

            return Array.AsReadOnly(normalized);
        }

        return value;
    }
}
