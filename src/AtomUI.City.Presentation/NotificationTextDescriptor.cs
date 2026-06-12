namespace AtomUI.City.Presentation;

public sealed class NotificationTextDescriptor
{
    public NotificationTextDescriptor(
        string notificationId,
        string? title = null,
        string? titleKey = null,
        string? message = null,
        string? messageKey = null,
        IReadOnlyList<object?>? messageArguments = null,
        string? actionText = null,
        string? actionTextKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notificationId);

        NotificationId = notificationId;
        Title = title;
        TitleKey = NormalizeKey(titleKey);
        Message = message;
        MessageKey = NormalizeKey(messageKey);
        MessageArguments = messageArguments;
        ActionText = actionText;
        ActionTextKey = NormalizeKey(actionTextKey);
    }

    public string NotificationId { get; }

    public string? Title { get; }

    public string? TitleKey { get; }

    public string? Message { get; }

    public string? MessageKey { get; }

    public IReadOnlyList<object?>? MessageArguments { get; }

    public string? ActionText { get; }

    public string? ActionTextKey { get; }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key;
    }
}
