using AtomUI.City.Routing;

namespace AtomUI.City.Security;

public interface IRouteAuthorizationPolicyProvider
{
    ValueTask<AuthorizationPolicy?> GetPolicyAsync(
        RouteGuardContext context,
        CancellationToken cancellationToken = default);
}
