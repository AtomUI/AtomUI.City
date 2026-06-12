using AtomUI.City.Localization;

namespace AtomUI.City.Presentation;

public interface IPresentationFlowDirectionTarget
{
    ValueTask<LocalizationResult> ApplyFlowDirectionAsync(
        PresentationFlowDirection direction,
        CultureState state,
        CancellationToken cancellationToken = default);
}
