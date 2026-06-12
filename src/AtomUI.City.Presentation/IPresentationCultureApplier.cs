using AtomUI.City.Localization;

namespace AtomUI.City.Presentation;

public interface IPresentationCultureApplier
{
    ValueTask<LocalizationResult> ApplyCultureAsync(
        CultureState state,
        CancellationToken cancellationToken = default);
}
