using System.Globalization;

namespace AtomUI.City.Localization;

public sealed class LocalizedMessage
{
    private LocalizedMessage(
        string key,
        string value,
        CultureInfo culture,
        bool isFallback,
        bool isMissing,
        bool isFormatFailed)
    {
        Key = key;
        Value = value;
        Culture = culture;
        IsFallback = isFallback;
        IsMissing = isMissing;
        IsFormatFailed = isFormatFailed;
    }

    public string Key { get; }

    public string Value { get; }

    public CultureInfo Culture { get; }

    public bool IsFallback { get; }

    public bool IsMissing { get; }

    public bool IsFormatFailed { get; }

    public static LocalizedMessage FromString(
        LocalizedString localizedString,
        string value,
        bool isFormatFailed = false)
    {
        ArgumentNullException.ThrowIfNull(localizedString);
        ArgumentNullException.ThrowIfNull(value);

        return new LocalizedMessage(
            localizedString.Key,
            value,
            localizedString.Culture,
            localizedString.IsFallback,
            localizedString.IsMissing,
            isFormatFailed);
    }
}
