using AtomUI.City.Localization;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class PresentationLocalizationBridge : IPresentationLocalizationBridge
{
    private readonly IUiDispatcher _dispatcher;
    private readonly IReadOnlyList<IPresentationCultureApplier> _appliers;

    public PresentationLocalizationBridge(
        IUiDispatcher dispatcher,
        IEnumerable<IPresentationCultureApplier> appliers)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(appliers);

        _dispatcher = dispatcher;
        _appliers = appliers.ToArray();
    }

    public async ValueTask<LocalizationResult> ApplyCultureAsync(
        CultureState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        var result = LocalizationResult.Success();

        try
        {
            await _dispatcher
                .PostAsync(
                    async dispatcherCancellationToken =>
                    {
                        foreach (var applier in _appliers)
                        {
                            dispatcherCancellationToken.ThrowIfCancellationRequested();

                            var applyResult = await applier
                                .ApplyCultureAsync(state, dispatcherCancellationToken)
                                .ConfigureAwait(false);

                            if (!applyResult.Succeeded)
                            {
                                result = applyResult;
                                return;
                            }
                        }
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            result = LocalizationResult.Failed(
                new LocalizationError(
                    LocalizationErrorKind.PresentationApplyFailed,
                    "Presentation culture apply failed.",
                    exception));
        }

        return result;
    }
}
