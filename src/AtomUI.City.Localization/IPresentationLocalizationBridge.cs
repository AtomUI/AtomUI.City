namespace AtomUI.City.Localization;

public interface IPresentationLocalizationBridge
{
    ValueTask<LocalizationResult> ApplyCultureAsync(
        CultureState state,
        CancellationToken cancellationToken = default);
}
