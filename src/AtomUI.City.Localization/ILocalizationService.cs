using System.Globalization;

namespace AtomUI.City.Localization;

public interface ILocalizationService
{
    CultureInfo CurrentCulture { get; }

    long CultureRevision { get; }

    ValueTask<LocalizationResult> SetCultureAsync(
        string cultureName,
        CancellationToken cancellationToken = default);

    ValueTask<LocalizedString> GetStringAsync(
        string key,
        CancellationToken cancellationToken = default);

    ValueTask<LocalizedMessage> GetMessageAsync(
        string key,
        IReadOnlyList<object?> arguments,
        CancellationToken cancellationToken = default);

    ValueTask<ILocalizedText> CreateTextAsync(
        string key,
        CancellationToken cancellationToken = default);

    ValueTask<ILocalizedText> CreateMessageTextAsync(
        string key,
        IReadOnlyList<object?> arguments,
        CancellationToken cancellationToken = default);
}
