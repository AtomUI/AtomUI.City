using AtomUI.City.Diagnostics;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class ValidationVisualStateBinding
{
    private readonly IUiDispatcher _dispatcher;
    private readonly IHostDiagnostics? _diagnostics;

    public ValidationVisualStateBinding(IUiDispatcher dispatcher)
        : this(dispatcher, diagnostics: null)
    {
    }

    public ValidationVisualStateBinding(
        IUiDispatcher dispatcher,
        IHostDiagnostics? diagnostics)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        _dispatcher = dispatcher;
        _diagnostics = diagnostics;
    }

    public async ValueTask ApplyAsync(
        ValidationScope scope,
        IValidationVisualStateTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(target);

        var snapshot = ValidationVisualStateSnapshot.From(scope);

        try
        {
            await _dispatcher
                .InvokeAsync(
                    () => target.ApplyValidationState(snapshot),
                    cancellationToken)
                .ConfigureAwait(false);

            WriteAppliedDiagnostic(snapshot);
        }
        catch (Exception exception)
        {
            WriteFailedDiagnostic(snapshot, exception);
            throw;
        }
    }

    private void WriteAppliedDiagnostic(ValidationVisualStateSnapshot snapshot)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ValidationVisualStateApplied,
            $"Presentation validation visual state applied status '{snapshot.Status}' with keys '{FormatKeys(snapshot)}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteFailedDiagnostic(
        ValidationVisualStateSnapshot snapshot,
        Exception exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ValidationVisualStateApplyFailed,
            $"Presentation validation visual state failed to apply status '{snapshot.Status}' with keys '{FormatKeys(snapshot)}': {exception.Message}",
            HostDiagnosticSeverity.Error));
    }

    private static string FormatKeys(ValidationVisualStateSnapshot snapshot)
    {
        return snapshot.Errors.Count == 0 ? "<none>" : string.Join(", ", snapshot.Errors.Keys);
    }
}
