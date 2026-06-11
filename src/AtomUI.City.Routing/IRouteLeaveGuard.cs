namespace AtomUI.City.Routing;

public interface IRouteLeaveGuard
{
    ValueTask<RouteGuardResult> CanLeaveAsync(
        RouteGuardContext context,
        CancellationToken cancellationToken);
}
