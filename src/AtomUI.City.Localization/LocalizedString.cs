using System.Globalization;

namespace AtomUI.City.Localization;

public sealed class LocalizedString
{
    private LocalizedString(
        string key,
        string value,
        CultureInfo culture,
        bool isFallback,
        bool isMissing)
    {
        Key = key;
        Value = value;
        Culture = culture;
        IsFallback = isFallback;
        IsMissing = isMissing;
    }

    public string Key { get; }

    public string Value { get; }

    public CultureInfo Culture { get; }

    public bool IsFallback { get; }

    public bool IsMissing { get; }

    public static LocalizedString Found(string key, string value, CultureInfo culture)
    {
        return new LocalizedString(key, value, culture, isFallback: false, isMissing: false);
    }

    public static LocalizedString Fallback(string key, string value, CultureInfo culture)
    {
        return new LocalizedString(key, value, culture, isFallback: true, isMissing: false);
    }

    public static LocalizedString Missing(string key, CultureInfo culture)
    {
        return new LocalizedString(key, $"!{key}!", culture, isFallback: false, isMissing: true);
    }
}
