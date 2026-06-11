namespace AtomUI.City.Presentation;

public sealed class CommandTextDescriptor
{
    public CommandTextDescriptor(
        string commandId,
        string? textKey = null,
        string? toolTipKey = null,
        string? descriptionKey = null,
        string? iconKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        CommandId = commandId;
        TextKey = NormalizeKey(textKey);
        ToolTipKey = NormalizeKey(toolTipKey);
        DescriptionKey = NormalizeKey(descriptionKey);
        IconKey = NormalizeKey(iconKey);
    }

    public string CommandId { get; }

    public string? TextKey { get; }

    public string? ToolTipKey { get; }

    public string? DescriptionKey { get; }

    public string? IconKey { get; }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key;
    }
}
