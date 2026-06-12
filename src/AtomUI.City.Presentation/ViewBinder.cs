using System.Diagnostics;
using AtomUI.City.Diagnostics;

namespace AtomUI.City.Presentation;

public sealed class ViewBinder
{
    private readonly IHostDiagnostics? _diagnostics;

    public ViewBinder()
    {
    }

    public ViewBinder(IHostDiagnostics diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        _diagnostics = diagnostics;
    }

    public BoundViewHandle Bind(
        ViewDescriptor descriptor,
        object view,
        object viewModel)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(viewModel);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (view is not IViewDataContextAware dataContextAware)
            {
                throw new PresentationException(
                    PresentationError.BindingFailed,
                    $"View '{view.GetType().FullName}' does not expose a Presentation data context contract.");
            }

            dataContextAware.DataContext = viewModel;

            var handle = BoundViewHandle.Create(
                descriptor,
                view,
                viewModel,
                () => dataContextAware.DataContext = null);

            stopwatch.Stop();
            WriteBoundDiagnostic(descriptor, view, stopwatch.Elapsed);

            return handle;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            WriteBindingFailedDiagnostic(descriptor, view, stopwatch.Elapsed, exception);

            throw;
        }
    }

    private void WriteBoundDiagnostic(
        ViewDescriptor descriptor,
        object view,
        TimeSpan elapsed)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ViewBound,
            $"View binder bound view '{view.GetType().FullName}' to view model '{descriptor.ViewModelType.FullName}' in {elapsed.TotalMilliseconds:0.###} ms.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteBindingFailedDiagnostic(
        ViewDescriptor descriptor,
        object view,
        TimeSpan elapsed,
        Exception exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ViewBindingFailed,
            $"View binder failed to bind view '{view.GetType().FullName}' to view model '{descriptor.ViewModelType.FullName}' in {elapsed.TotalMilliseconds:0.###} ms: {exception.Message}",
            HostDiagnosticSeverity.Error));
    }
}
