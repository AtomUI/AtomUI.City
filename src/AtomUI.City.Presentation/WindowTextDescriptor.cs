namespace AtomUI.City.Presentation;

public sealed class WindowTextDescriptor
{
    public WindowTextDescriptor(
        string windowId,
        string? title = null,
        string? titleKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(windowId);

        WindowId = windowId;
        Title = title;
        TitleKey = NormalizeKey(titleKey);
    }

    public string WindowId { get; }

    public string? Title { get; }

    public string? TitleKey { get; }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key;
    }
}
