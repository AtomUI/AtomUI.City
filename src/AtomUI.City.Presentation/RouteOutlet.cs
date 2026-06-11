using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class RouteOutlet : IRouteOutlet
{
    private readonly IUiDispatcher _dispatcher;
    private BoundViewHandle? _currentHandle;

    public RouteOutlet(string name, IUiDispatcher dispatcher)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(dispatcher);

        Name = name;
        _dispatcher = dispatcher;
    }

    public string Name { get; }

    public object? CurrentContent => _currentHandle?.View;

    public async ValueTask<RouteOutletCommitResult> CommitAsync(
        RouteOutletCommitPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (!string.Equals(plan.OutletName, Name, StringComparison.Ordinal))
        {
            plan.Handle?.Dispose();

            return RouteOutletCommitResult.Failed(
                PresentationError.OutletNotFound,
                $"Outlet '{plan.OutletName}' was not found.");
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

            return RouteOutletCommitResult.Success();
        }
        catch (Exception exception)
        {
            plan.Handle?.Dispose();

            return RouteOutletCommitResult.Failed(
                PresentationError.OutletCommitFailed,
                exception.Message);
        }
    }
}
