using AtomUI.City.Diagnostics;
using AtomUI.City.Localization;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class PresentationResourceDictionaryRevoker : IPresentationResourceDictionaryRevoker
{
    private readonly IUiDispatcher _dispatcher;
    private readonly IReadOnlyList<IPresentationResourceDictionaryTarget> _targets;
    private readonly IHostDiagnostics? _diagnostics;

    public PresentationResourceDictionaryRevoker(
        IUiDispatcher dispatcher,
        IEnumerable<IPresentationResourceDictionaryTarget> targets)
        : this(dispatcher, targets, diagnostics: null)
    {
    }

    public PresentationResourceDictionaryRevoker(
        IUiDispatcher dispatcher,
        IEnumerable<IPresentationResourceDictionaryTarget> targets,
        IHostDiagnostics? diagnostics)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(targets);

        _dispatcher = dispatcher;
        _targets = targets.ToArray();
        _diagnostics = diagnostics;
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

        WriteRevocationDiagnostic(revocation, result);

        return result;
    }

    private void WriteRevocationDiagnostic(
        PresentationResourceDictionaryRevocation revocation,
        LocalizationResult result)
    {
        if (result.Succeeded)
        {
            _diagnostics?.Write(new HostDiagnosticRecord(
                PresentationDiagnosticIds.ResourceDictionaryRevoked,
                $"Presentation resource dictionary revoked plugin '{revocation.PluginId}' contribution '{NormalizeContributionId(revocation.ContributionId)}'.",
                HostDiagnosticSeverity.Info));

            return;
        }

        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ResourceDictionaryRevokeFailed,
            $"Presentation resource dictionary failed to revoke plugin '{revocation.PluginId}' contribution '{NormalizeContributionId(revocation.ContributionId)}': {result.Error?.Message}",
            HostDiagnosticSeverity.Error));
    }

    private static string NormalizeContributionId(string? contributionId)
    {
        return string.IsNullOrWhiteSpace(contributionId) ? "<all>" : contributionId;
    }
}
