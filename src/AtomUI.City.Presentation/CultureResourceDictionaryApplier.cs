using AtomUI.City.Diagnostics;
using AtomUI.City.Localization;

namespace AtomUI.City.Presentation;

public sealed class CultureResourceDictionaryApplier : IPresentationCultureApplier
{
    private readonly IReadOnlyList<IPresentationResourceDictionaryTarget> _targets;
    private readonly IHostDiagnostics? _diagnostics;

    public CultureResourceDictionaryApplier(IEnumerable<IPresentationResourceDictionaryTarget> targets)
        : this(targets, diagnostics: null)
    {
    }

    public CultureResourceDictionaryApplier(
        IEnumerable<IPresentationResourceDictionaryTarget> targets,
        IHostDiagnostics? diagnostics)
    {
        ArgumentNullException.ThrowIfNull(targets);

        _targets = targets.ToArray();
        _diagnostics = diagnostics;
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
                WriteApplyFailedDiagnostic(state, result);

                return result;
            }
        }

        var success = LocalizationResult.Success();
        WriteAppliedDiagnostic(state);

        return success;
    }

    private void WriteAppliedDiagnostic(CultureState state)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ResourceDictionaryApplied,
            $"Presentation resource dictionary applied culture '{state.CurrentCulture.Name}' with packages '{FormatPackageIds(state.LoadedPackageIds)}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteApplyFailedDiagnostic(
        CultureState state,
        LocalizationResult result)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ResourceDictionaryApplyFailed,
            $"Presentation resource dictionary failed to apply culture '{state.CurrentCulture.Name}' with packages '{FormatPackageIds(state.LoadedPackageIds)}': {result.Error?.Message}",
            HostDiagnosticSeverity.Error));
    }

    private static string FormatPackageIds(IReadOnlyList<string> packageIds)
    {
        return packageIds.Count == 0 ? "<none>" : string.Join(", ", packageIds);
    }
}
