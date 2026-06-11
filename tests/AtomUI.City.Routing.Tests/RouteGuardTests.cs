using AtomUI.City.Routing;

namespace AtomUI.City.Routing.Tests;

public sealed class RouteGuardTests
{
    [Fact]
    public async Task EnterGuardRejectsNavigationAndKeepsCurrentSnapshot()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Route(
                    "settings",
                    "settings",
                    typeof(SettingsViewModel),
                    enterGuardTypes: [typeof(RejectEnterGuard)]),
            ]);
        var scope = new NavigationScope(graph, ResolveGuard);

        var result = await scope.Router.NavigateByPathAsync("settings");

        Assert.Equal(NavigationResultStatus.Rejected, result.Status);
        Assert.Equal("settings-disabled", result.Error?.Code);
        Assert.Null(scope.CurrentSnapshot.ActiveRoute);
    }

    [Fact]
    public async Task LeaveGuardRejectsNavigationAndKeepsPreviousSnapshot()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Route(
                    "home",
                    "home",
                    typeof(HomeViewModel),
                    leaveGuardTypes: [typeof(RejectLeaveGuard)]),
                Route("settings", "settings", typeof(SettingsViewModel)),
            ]);
        var scope = new NavigationScope(graph, ResolveGuard);

        var first = await scope.Router.NavigateByPathAsync("home");
        var second = await scope.Router.NavigateByPathAsync("settings");

        Assert.Equal(NavigationResultStatus.Success, first.Status);
        Assert.Equal(NavigationResultStatus.Rejected, second.Status);
        Assert.Equal("unsaved-changes", second.Error?.Code);
        Assert.Equal("home", scope.CurrentSnapshot.Route.RouteId);
    }

    [Fact]
    public async Task MatchPolicySkipsRejectedCandidateAndTriesNextRoute()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Route(
                    "settings.disabled",
                    "settings",
                    typeof(DisabledSettingsViewModel),
                    matchPolicyTypes: [typeof(DisabledMatchPolicy)]),
                Route("settings.enabled", "settings", typeof(SettingsViewModel)),
            ]);
        var scope = new NavigationScope(graph, ResolveGuard);

        var result = await scope.Router.NavigateByPathAsync("settings");

        Assert.Equal(NavigationResultStatus.Success, result.Status);
        Assert.Equal("settings.enabled", result.Route.RouteId);
        Assert.Equal(typeof(SettingsViewModel), result.Route.ViewModelTarget?.ViewModelType);
    }

    [Fact]
    public async Task GuardExceptionReturnsFailedResultAndKeepsCurrentSnapshot()
    {
        var graph = RouteGraphSnapshot.Create(
            [
                Route(
                    "settings",
                    "settings",
                    typeof(SettingsViewModel),
                    enterGuardTypes: [typeof(ThrowingEnterGuard)]),
            ]);
        var scope = new NavigationScope(graph, ResolveGuard);

        var result = await scope.Router.NavigateByPathAsync("settings");

        Assert.Equal(NavigationResultStatus.Failed, result.Status);
        Assert.Equal("CITY-NAVIGATION-FAILED", result.Error?.Code);
        Assert.IsType<InvalidOperationException>(result.Error?.Exception);
        Assert.Null(scope.CurrentSnapshot.ActiveRoute);
    }

    private static object ResolveGuard(Type type)
    {
        if (type == typeof(RejectEnterGuard))
        {
            return new RejectEnterGuard();
        }

        if (type == typeof(RejectLeaveGuard))
        {
            return new RejectLeaveGuard();
        }

        if (type == typeof(DisabledMatchPolicy))
        {
            return new DisabledMatchPolicy();
        }

        if (type == typeof(ThrowingEnterGuard))
        {
            return new ThrowingEnterGuard();
        }

        throw new InvalidOperationException($"Unsupported guard type '{type.FullName}'.");
    }

    private static RouteDescriptor Route(
        string id,
        string template,
        Type viewModelType,
        IReadOnlyList<Type>? enterGuardTypes = null,
        IReadOnlyList<Type>? leaveGuardTypes = null,
        IReadOnlyList<Type>? matchPolicyTypes = null)
    {
        return new RouteDescriptor(
            id,
            RouteDefinitionKind.Route,
            template,
            new ViewModelTargetDescriptor(viewModelType),
            parentRouteId: null,
            enterGuardTypes: enterGuardTypes,
            leaveGuardTypes: leaveGuardTypes,
            matchPolicyTypes: matchPolicyTypes);
    }

    private sealed class RejectEnterGuard : IRouteEnterGuard
    {
        public ValueTask<RouteGuardResult> CanEnterAsync(
            RouteGuardContext context,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(RouteGuardResult.Reject("settings-disabled"));
        }
    }

    private sealed class RejectLeaveGuard : IRouteLeaveGuard
    {
        public ValueTask<RouteGuardResult> CanLeaveAsync(
            RouteGuardContext context,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(RouteGuardResult.Reject("unsaved-changes"));
        }
    }

    private sealed class DisabledMatchPolicy : IRouteMatchPolicy
    {
        public ValueTask<bool> CanMatchAsync(
            RouteMatchPolicyContext context,
            CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(false);
        }
    }

    private sealed class ThrowingEnterGuard : IRouteEnterGuard
    {
        public ValueTask<RouteGuardResult> CanEnterAsync(
            RouteGuardContext context,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Guard failed.");
        }
    }

    private sealed class HomeViewModel;

    private sealed class SettingsViewModel;

    private sealed class DisabledSettingsViewModel;
}
