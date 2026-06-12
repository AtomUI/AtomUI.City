using AtomUI.City.Localization;

namespace AtomUI.City.Presentation;

public sealed class CultureResourceDictionaryApplier : IPresentationCultureApplier
{
    private readonly IReadOnlyList<IPresentationResourceDictionaryTarget> _targets;

    public CultureResourceDictionaryApplier(IEnumerable<IPresentationResourceDictionaryTarget> targets)
    {
        ArgumentNullException.ThrowIfNull(targets);

        _targets = targets.ToArray();
    }

    public async ValueTask<LocalizationResult> ApplyCultureAsync(
        CultureState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        foreach (var target in _targets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await target
                .ApplyResourcesAsync(state, cancellationToken)
                .ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return result;
            }
        }

        return LocalizationResult.Success();
    }
}
