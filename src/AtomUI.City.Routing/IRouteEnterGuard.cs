namespace AtomUI.City.Routing;

public interface IRouteEnterGuard
{
    ValueTask<RouteGuardResult> CanEnterAsync(
        RouteGuardContext context,
        CancellationToken cancellationToken);
}
