using System.Security.Claims;
using AtomUI.City.Routing;
using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class RouteAuthorizationGuardTests
{
    [Fact]
    public async Task GuardAllowsRouteWithoutAuthorizationPolicy()
    {
        var guard = CreateGuard();
        var context = CreateContext("home");

        var result = await guard.CanEnterAsync(context, CancellationToken.None);

        Assert.Equal(RouteGuardResultStatus.Allow, result.Status);
    }

    [Fact]
    public async Task GuardRejectsProtectedRouteForAnonymousPrincipal()
    {
        var guard = CreateGuard(policyProvider =>
        {
            policyProvider.Add("settings", AuthorizationPolicy.RequireAuthenticated("SignedIn"));
        });
        var context = CreateContext("settings");

        var result = await guard.CanEnterAsync(context, CancellationToken.None);

        Assert.Equal(RouteGuardResultStatus.Reject, result.Status);
        Assert.Equal(SecurityRouteGuardResultCodes.AuthenticationRequired, result.Code);
    }

    [Fact]
    public async Task GuardRejectsForbiddenRouteForAuthenticatedPrincipalWithoutPermission()
    {
        var store = new AuthenticationStateStore();
        store.SetAuthenticated(CreatePrincipal(permissions: []));
        var permissions = new PermissionRegistry();
        permissions.Add(new PermissionDescriptor("settings.read"));
        var guard = CreateGuard(
            policyProvider =>
            {
                policyProvider.Add("settings", AuthorizationPolicy.RequirePermission("CanReadSettings", "settings.read"));
            },
            store,
            permissions);
        var context = CreateContext("settings");

        var result = await guard.CanEnterAsync(context, CancellationToken.None);

        Assert.Equal(RouteGuardResultStatus.Reject, result.Status);
        Assert.Equal(SecurityRouteGuardResultCodes.Forbidden, result.Code);
    }

    [Fact]
    public async Task GuardAllowsAuthorizedRoute()
    {
        var store = new AuthenticationStateStore();
        store.SetAuthenticated(CreatePrincipal(permissions: ["settings.read"]));
        var permissions = new PermissionRegistry();
        permissions.Add(new PermissionDescriptor("settings.read"));
        var guard = CreateGuard(
            policyProvider =>
            {
                policyProvider.Add("settings", AuthorizationPolicy.RequirePermission("CanReadSettings", "settings.read"));
            },
            store,
            permissions);
        var context = CreateContext("settings");

        var result = await guard.CanEnterAsync(context, CancellationToken.None);

        Assert.Equal(RouteGuardResultStatus.Allow, result.Status);
    }

    [Fact]
    public async Task GuardCancellationDoesNotAllowNavigation()
    {
        var guard = CreateGuard(policyProvider =>
        {
            policyProvider.Add("settings", AuthorizationPolicy.RequireAuthenticated("SignedIn"));
        });
        var context = CreateContext("settings");
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await guard.CanEnterAsync(context, cancellation.Token);

        Assert.Equal(RouteGuardResultStatus.Cancel, result.Status);
    }

    private static SecurityRouteGuard CreateGuard(
        Action<InMemoryRouteAuthorizationPolicyProvider>? configurePolicyProvider = null,
        AuthenticationStateStore? store = null,
        PermissionRegistry? permissions = null)
    {
        var policyProvider = new InMemoryRouteAuthorizationPolicyProvider();
        configurePolicyProvider?.Invoke(policyProvider);
        store ??= new AuthenticationStateStore();
        permissions ??= new PermissionRegistry();

        return new SecurityRouteGuard(
            new AuthorizationEvaluator(permissions),
            store,
            policyProvider);
    }

    private static RouteGuardContext CreateContext(string routeId)
    {
        var route = new RouteDescriptor(
            routeId,
            RouteDefinitionKind.Route,
            routeId,
            new ViewModelTargetDescriptor(typeof(TestViewModel)));
        var target = NavigationTarget.FromRouteReference(routeId, parameters: null, NavigationOptions.Default);

        return new RouteGuardContext(
            Guid.NewGuid(),
            target,
            route,
            NavigationSnapshot.Empty(0),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    private static ClaimsPrincipal CreatePrincipal(IReadOnlyCollection<string> permissions)
    {
        var identity = new ClaimsIdentity(authenticationType: "Test");

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim("permission", permission));
        }

        return new ClaimsPrincipal(identity);
    }

    private sealed class TestViewModel;
}
