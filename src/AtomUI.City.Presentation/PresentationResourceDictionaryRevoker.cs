using AtomUI.City.Localization;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class PresentationResourceDictionaryRevoker : IPresentationResourceDictionaryRevoker
{
    private readonly IUiDispatcher _dispatcher;
    private readonly IReadOnlyList<IPresentationResourceDictionaryTarget> _targets;

    public PresentationResourceDictionaryRevoker(
        IUiDispatcher dispatcher,
        IEnumerable<IPresentationResourceDictionaryTarget> targets)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(targets);

        _dispatcher = dispatcher;
        _targets = targets.ToArray();
    }

    public async ValueTask<LocalizationResult> RevokeAsync(
        PresentationResourceDictionaryRevocation revocation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(revocation);

        var result = LocalizationResult.Success();

        try
        {
            await _dispatcher
                .PostAsync(
                    async dispatcherCancellationToken =>
                    {
                        foreach (var target in _targets)
                        {
                            dispatcherCancellationToken.ThrowIfCancellationRequested();

                            var revokeResult = await target
                                .RevokeResourcesAsync(revocation, dispatcherCancellationToken)
                                .ConfigureAwait(false);

                            if (!revokeResult.Succeeded)
                            {
                                result = revokeResult;
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
                    "Presentation resource dictionary revoke failed.",
                    exception));
        }

        return result;
    }
}
