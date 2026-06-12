using AtomUI.City.Localization;

namespace AtomUI.City.Presentation;

public interface IPresentationResourceDictionaryTarget
{
    ValueTask<LocalizationResult> ApplyResourcesAsync(
        CultureState state,
        CancellationToken cancellationToken = default);
}
