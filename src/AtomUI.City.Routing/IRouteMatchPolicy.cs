namespace AtomUI.City.Routing;

public interface IRouteMatchPolicy
{
    ValueTask<bool> CanMatchAsync(
        RouteMatchPolicyContext context,
        CancellationToken cancellationToken);
}
