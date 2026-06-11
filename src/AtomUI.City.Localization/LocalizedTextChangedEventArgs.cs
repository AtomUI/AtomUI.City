using System.Globalization;

namespace AtomUI.City.Localization;

public sealed class LocalizedTextChangedEventArgs : EventArgs
{
    public LocalizedTextChangedEventArgs(LocalizedString text, long revision)
    {
        ArgumentNullException.ThrowIfNull(text);

        Key = text.Key;
        Value = text.Value;
        Culture = text.Culture;
        IsFallback = text.IsFallback;
        IsMissing = text.IsMissing;
        Revision = revision;
    }

    public string Key { get; }

    public string Value { get; }

    public CultureInfo Culture { get; }

    public bool IsFallback { get; }

    public bool IsMissing { get; }

    public long Revision { get; }
}
