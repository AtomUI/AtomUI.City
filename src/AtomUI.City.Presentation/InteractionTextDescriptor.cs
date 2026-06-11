namespace AtomUI.City.Presentation;

public sealed class InteractionTextDescriptor
{
    public InteractionTextDescriptor(
        string? titleKey = null,
        string? messageKey = null,
        string? primaryActionKey = null,
        string? secondaryActionKey = null,
        string? cancelActionKey = null)
    {
        TitleKey = NormalizeKey(titleKey);
        MessageKey = NormalizeKey(messageKey);
        PrimaryActionKey = NormalizeKey(primaryActionKey);
        SecondaryActionKey = NormalizeKey(secondaryActionKey);
        CancelActionKey = NormalizeKey(cancelActionKey);
    }

    public string? TitleKey { get; }

    public string? MessageKey { get; }

    public string? PrimaryActionKey { get; }

    public string? SecondaryActionKey { get; }

    public string? CancelActionKey { get; }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key;
    }
}
