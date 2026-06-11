using System.Globalization;

namespace AtomUI.City.Localization;

public interface ILocalizedText : IDisposable
{
    event EventHandler<LocalizedTextChangedEventArgs>? Changed;

    string Key { get; }

    string Value { get; }

    CultureInfo Culture { get; }

    long Revision { get; }

    bool IsFallback { get; }

    bool IsMissing { get; }

    ValueTask RefreshAsync(CancellationToken cancellationToken = default);
}
