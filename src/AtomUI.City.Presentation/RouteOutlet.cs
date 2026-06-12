using AtomUI.City.Diagnostics;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class RouteOutlet : IRouteOutlet
{
    private readonly IUiDispatcher _dispatcher;
    private readonly IHostDiagnostics? _diagnostics;
    private BoundViewHandle? _currentHandle;

    public RouteOutlet(string name, IUiDispatcher dispatcher)
        : this(name, dispatcher, diagnostics: null)
    {
    }

    public RouteOutlet(
        string name,
        IUiDispatcher dispatcher,
        IHostDiagnostics? diagnostics)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(dispatcher);

        Name = name;
        _dispatcher = dispatcher;
        _diagnostics = diagnostics;
    }

    public string Name { get; }

    public object? CurrentContent => _currentHandle?.View;

    public async ValueTask<RouteOutletCommitResult> CommitAsync(
        RouteOutletCommitPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        WriteCommitPlannedDiagnostic(plan);

        if (!string.Equals(plan.OutletName, Name, StringComparison.Ordinal))
        {
            plan.Handle?.Dispose();

            var result = RouteOutletCommitResult.Failed(
                PresentationError.OutletNotFound,
                $"Outlet '{plan.OutletName}' was not found.");
            WriteCommitFailedDiagnostic(plan, result);

            return result;
        }

        var previousHandle = _currentHandle;

        try
        {
            await _dispatcher.InvokeAsync(
                () =>
                {
                    _currentHandle = plan.Operation == RouteOutletOperation.Clear
                        ? null
                        : plan.Handle;
                    previousHandle?.Dispose();
                },
                cancellationToken);

            var result = RouteOutletCommitResult.Success();
            WriteCommitSucceededDiagnostic(plan);

            return result;
        }
        catch (Exception exception)
        {
            plan.Handle?.Dispose();

            var result = RouteOutletCommitResult.Failed(
                PresentationError.OutletCommitFailed,
                exception.Message);
            WriteCommitFailedDiagnostic(plan, result);

            return result;
        }
    }

    private void WriteCommitPlannedDiagnostic(RouteOutletCommitPlan plan)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.OutletCommitPlanned,
            $"Route outlet '{Name}' received {plan.Operation} commit plan for outlet '{plan.OutletName}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteCommitSucceededDiagnostic(RouteOutletCommitPlan plan)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.OutletCommitSucceeded,
            $"Route outlet '{Name}' completed {plan.Operation} commit for outlet '{plan.OutletName}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteCommitFailedDiagnostic(
        RouteOutletCommitPlan plan,
        RouteOutletCommitResult result)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.OutletCommitFailed,
            $"Route outlet '{Name}' failed {plan.Operation} commit for outlet '{plan.OutletName}' with error '{result.Error}': {result.Message}",
            HostDiagnosticSeverity.Error));
    }
}
