using AtomUI.City.Diagnostics;

namespace AtomUI.City.Presentation;

public sealed class PresentationPluginUnloadCoordinator : IPresentationPluginUnloadCoordinator
{
    private readonly IActivePluginViewRegistry _activeViews;
    private readonly IInteractionHandlerRegistry _interactionHandlers;
    private readonly IPresentationResourceRegistry _resources;
    private readonly IPresentationResourceDictionaryRevoker _resourceDictionaries;
    private readonly IHostDiagnostics? _diagnostics;

    public PresentationPluginUnloadCoordinator(
        IActivePluginViewRegistry activeViews,
        IInteractionHandlerRegistry interactionHandlers,
        IPresentationResourceRegistry resources,
        IPresentationResourceDictionaryRevoker resourceDictionaries,
        IHostDiagnostics? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(activeViews);
        ArgumentNullException.ThrowIfNull(interactionHandlers);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(resourceDictionaries);

        _activeViews = activeViews;
        _interactionHandlers = interactionHandlers;
        _resources = resources;
        _resourceDictionaries = resourceDictionaries;
        _diagnostics = diagnostics;
    }

    public async ValueTask<PresentationPluginUnloadResult> CleanupAsync(
        PresentationPluginUnloadRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new List<PresentationPluginUnloadError>();
        var closedViewCount = await CloseActiveViewsAsync(request, cancellationToken).ConfigureAwait(false);

        if (HasRemainingActiveViews(request))
        {
            errors.Add(new PresentationPluginUnloadError(
                PresentationPluginUnloadErrorKind.ActiveViewsRemaining,
                "Presentation plugin unload cleanup stopped because active plugin views remain."));

            return Complete(
                request,
                closedViewCount,
                revokedInteractionHandlerCount: 0,
                revokedResourceContributionCount: 0,
                resourceDictionariesRevoked: false,
                errors);
        }

        var revokedInteractionHandlerCount = RevokeInteractionHandlers(request, errors);
        var resourceDictionaryResult = await _resourceDictionaries
            .RevokeAsync(
                new PresentationResourceDictionaryRevocation(
                    request.PluginId,
                    request.ContributionId),
                cancellationToken)
            .ConfigureAwait(false);

        if (!resourceDictionaryResult.Succeeded)
        {
            errors.Add(new PresentationPluginUnloadError(
                PresentationPluginUnloadErrorKind.ResourceDictionaryRevokeFailed,
                resourceDictionaryResult.Error?.Message ?? "Presentation resource dictionary revoke failed.",
                resourceDictionaryResult.Error?.Exception));
        }

        var revokedResourceContributionCount = RevokeResourceContributions(request, errors);

        return Complete(
            request,
            closedViewCount,
            revokedInteractionHandlerCount,
            revokedResourceContributionCount,
            resourceDictionaryResult.Succeeded,
            errors);
    }

    private ValueTask<int> CloseActiveViewsAsync(
        PresentationPluginUnloadRequest request,
        CancellationToken cancellationToken)
    {
        return request.ContributionId is null
            ? _activeViews.ClosePluginViewsAsync(request.PluginId, cancellationToken)
            : _activeViews.CloseContributionViewsAsync(request.ContributionId, cancellationToken);
    }

    private bool HasRemainingActiveViews(PresentationPluginUnloadRequest request)
    {
        return _activeViews.ActiveViews.Any(view => Matches(request, view));
    }

    private static bool Matches(
        PresentationPluginUnloadRequest request,
        ActivePluginView view)
    {
        return request.ContributionId is null
            ? string.Equals(view.PluginId, request.PluginId, StringComparison.Ordinal)
            : string.Equals(view.ContributionId, request.ContributionId, StringComparison.Ordinal);
    }

    private int RevokeInteractionHandlers(
        PresentationPluginUnloadRequest request,
        ICollection<PresentationPluginUnloadError> errors)
    {
        try
        {
            return request.ContributionId is null
                ? _interactionHandlers.RevokePlugin(request.PluginId)
                : _interactionHandlers.RevokeContribution(request.ContributionId);
        }
        catch (Exception exception)
        {
            errors.Add(new PresentationPluginUnloadError(
                PresentationPluginUnloadErrorKind.InteractionHandlerRevokeFailed,
                exception.Message,
                exception));

            return 0;
        }
    }

    private int RevokeResourceContributions(
        PresentationPluginUnloadRequest request,
        ICollection<PresentationPluginUnloadError> errors)
    {
        try
        {
            return request.ContributionId is null
                ? _resources.RevokePlugin(request.PluginId)
                : _resources.RevokeContribution(request.ContributionId);
        }
        catch (Exception exception)
        {
            errors.Add(new PresentationPluginUnloadError(
                PresentationPluginUnloadErrorKind.ResourceContributionRevokeFailed,
                exception.Message,
                exception));

            return 0;
        }
    }

    private PresentationPluginUnloadResult Complete(
        PresentationPluginUnloadRequest request,
        int closedViewCount,
        int revokedInteractionHandlerCount,
        int revokedResourceContributionCount,
        bool resourceDictionariesRevoked,
        IReadOnlyList<PresentationPluginUnloadError> errors)
    {
        var result = new PresentationPluginUnloadResult(
            request.PluginId,
            request.ContributionId,
            closedViewCount,
            revokedInteractionHandlerCount,
            revokedResourceContributionCount,
            resourceDictionariesRevoked,
            errors);

        WriteDiagnostic(result);

        return result;
    }

    private void WriteDiagnostic(PresentationPluginUnloadResult result)
    {
        var contributionId = Normalize(result.ContributionId);
        if (result.Succeeded)
        {
            _diagnostics?.Write(new HostDiagnosticRecord(
                PresentationDiagnosticIds.PluginUnloadCleanupCompleted,
                $"Presentation plugin unload cleanup completed plugin '{result.PluginId}' contribution '{contributionId}' closed views '{result.ClosedViewCount}' revoked interaction handlers '{result.RevokedInteractionHandlerCount}' revoked resource contributions '{result.RevokedResourceContributionCount}'.",
                HostDiagnosticSeverity.Info));

            return;
        }

        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.PluginUnloadCleanupFailed,
            $"Presentation plugin unload cleanup failed plugin '{result.PluginId}' contribution '{contributionId}': {FormatErrors(result.Errors)}",
            HostDiagnosticSeverity.Error));
    }

    private static string FormatErrors(IEnumerable<PresentationPluginUnloadError> errors)
    {
        return string.Join(
            "; ",
            errors.Select(error => $"{error.Kind}: {error.Message}"));
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "<all>" : value;
    }
}
