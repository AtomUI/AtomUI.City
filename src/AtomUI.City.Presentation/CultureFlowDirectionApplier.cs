using AtomUI.City.Localization;

namespace AtomUI.City.Presentation;

public sealed class CultureFlowDirectionApplier : IPresentationCultureApplier
{
    private readonly IReadOnlyList<IPresentationFlowDirectionTarget> _targets;

    public CultureFlowDirectionApplier(IEnumerable<IPresentationFlowDirectionTarget> targets)
    {
        ArgumentNullException.ThrowIfNull(targets);

        _targets = targets.ToArray();
    }

    public async ValueTask<LocalizationResult> ApplyCultureAsync(
        CultureState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        var direction = state.CurrentUICulture.TextInfo.IsRightToLeft
            ? PresentationFlowDirection.RightToLeft
            : PresentationFlowDirection.LeftToRight;

        foreach (var target in _targets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await target
                .ApplyFlowDirectionAsync(direction, state, cancellationToken)
                .ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return result;
            }
        }

        return LocalizationResult.Success();
    }
}
