namespace AtomUI.City.Routing;

public sealed class NavigationScope : IRouter
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly RouteGraphSnapshot _routeGraph;
    private readonly Func<Type, object?> _serviceResolver;

    public NavigationScope(
        RouteGraphSnapshot routeGraph,
        Func<Type, object?>? serviceResolver = null)
    {
        ArgumentNullException.ThrowIfNull(routeGraph);

        _routeGraph = routeGraph;
        _serviceResolver = serviceResolver ?? (_ => null);
        CurrentSnapshot = NavigationSnapshot.Empty(routeGraph.Version);
    }

    public IRouter Router => this;

    public NavigationSnapshot CurrentSnapshot { get; private set; }

    public ValueTask<NavigationResult> NavigateAsync(
        RouteReference route,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var target = NavigationTarget.FromRouteReference(
            route.Id,
            parameters: null,
            options ?? NavigationOptions.Default);

        return NavigateCoreAsync(target, cancellationToken);
    }

    public ValueTask<NavigationResult> NavigateAsync<TParameters>(
        RouteReference<TParameters> route,
        TParameters parameters,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var target = NavigationTarget.FromRouteReference(
            route.Id,
            route.BindParameters(parameters),
            options ?? NavigationOptions.Default);

        return NavigateCoreAsync(target, cancellationToken);
    }

    public ValueTask<NavigationResult> NavigateByPathAsync(
        string path,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var target = NavigationTarget.FromPath(path, options ?? NavigationOptions.Default);

        return NavigateCoreAsync(target, cancellationToken);
    }

    public ValueTask<NavigationResult> BackAsync(CancellationToken cancellationToken = default)
    {
        var target = NavigationTarget.FromJournal(NavigationOptions.Default);

        return ValueTask.FromResult(
            NavigationResult.Rejected(
                Guid.NewGuid(),
                target,
                "CITY-NAVIGATION-JOURNAL-NOT-AVAILABLE",
                "Navigation journal is not available yet."));
    }

    public ValueTask<NavigationResult> ForwardAsync(CancellationToken cancellationToken = default)
    {
        var target = NavigationTarget.FromJournal(NavigationOptions.Default);

        return ValueTask.FromResult(
            NavigationResult.Rejected(
                Guid.NewGuid(),
                target,
                "CITY-NAVIGATION-JOURNAL-NOT-AVAILABLE",
                "Navigation journal is not available yet."));
    }

    private async ValueTask<NavigationResult> NavigateCoreAsync(
        NavigationTarget target,
        CancellationToken cancellationToken)
    {
        var navigationId = Guid.NewGuid();
        var acquiredGate = false;

        try
        {
            await _gate.WaitAsync(cancellationToken);
            acquiredGate = true;

            cancellationToken.ThrowIfCancellationRequested();

            return target.Kind switch
            {
                NavigationTargetKind.Path => await NavigateByMatchedPathAsync(navigationId, target, cancellationToken),
                NavigationTargetKind.RouteReference => await NavigateByRouteIdAsync(navigationId, target, cancellationToken),
                _ => NavigationResult.Rejected(
                    navigationId,
                    target,
                    "CITY-NAVIGATION-TARGET-UNSUPPORTED",
                    $"Navigation target kind '{target.Kind}' is not supported yet."),
            };
        }
        catch (OperationCanceledException)
        {
            return NavigationResult.Cancelled(navigationId, target);
        }
        catch (Exception exception)
        {
            return NavigationResult.Failed(
                navigationId,
                target,
                "CITY-NAVIGATION-FAILED",
                exception.Message,
                exception);
        }
        finally
        {
            if (acquiredGate)
            {
                _gate.Release();
            }
        }
    }

    private async ValueTask<NavigationResult> NavigateByMatchedPathAsync(
        Guid navigationId,
        NavigationTarget target,
        CancellationToken cancellationToken)
    {
        foreach (var match in _routeGraph.Matcher.MatchAll(target.Path!))
        {
            if (!await CanMatchAsync(navigationId, target, match.Route, cancellationToken))
            {
                continue;
            }

            return await CompleteNavigationAsync(
                navigationId,
                target,
                match.Route,
                match.Parameters,
                cancellationToken);
        }

        return NavigationResult.NotFound(
            navigationId,
            target,
            $"No route matched path '{target.Path}'.");
    }

    private async ValueTask<NavigationResult> NavigateByRouteIdAsync(
        Guid navigationId,
        NavigationTarget target,
        CancellationToken cancellationToken)
    {
        if (!_routeGraph.TryGetRoute(target.RouteId!, out var route) || route is null)
        {
            return NavigationResult.NotFound(
                navigationId,
                target,
                $"No route with id '{target.RouteId}' was found.");
        }

        if (!await CanMatchAsync(navigationId, target, route, cancellationToken))
        {
            return NavigationResult.Rejected(
                navigationId,
                target,
                "CITY-NAVIGATION-MATCH-REJECTED",
                $"Route '{route.RouteId}' was rejected by a match policy.");
        }

        return await CompleteNavigationAsync(
            navigationId,
            target,
            route,
            target.Parameters,
            cancellationToken);
    }

    private async ValueTask<NavigationResult> CompleteNavigationAsync(
        Guid navigationId,
        NavigationTarget target,
        RouteDescriptor route,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        if (CurrentSnapshot.ActiveRoute is not null &&
            !string.Equals(CurrentSnapshot.ActiveRoute.RouteId, route.RouteId, StringComparison.Ordinal))
        {
            var leaveResult = await RunLeaveGuardsAsync(navigationId, target, CurrentSnapshot.ActiveRoute, parameters, cancellationToken);

            if (leaveResult.Status != RouteGuardResultStatus.Allow)
            {
                return MapGuardResult(navigationId, target, leaveResult);
            }
        }

        var enterResult = await RunEnterGuardsAsync(navigationId, target, route, parameters, cancellationToken);

        if (enterResult.Status != RouteGuardResultStatus.Allow)
        {
            return MapGuardResult(navigationId, target, enterResult);
        }

        CurrentSnapshot = NavigationSnapshot.FromRoute(route, parameters, _routeGraph.Version);

        return NavigationResult.Success(navigationId, target, route, parameters);
    }

    private async ValueTask<bool> CanMatchAsync(
        Guid navigationId,
        NavigationTarget target,
        RouteDescriptor route,
        CancellationToken cancellationToken)
    {
        foreach (var policyType in route.MatchPolicyTypes)
        {
            var policy = Resolve<IRouteMatchPolicy>(policyType);
            var context = new RouteMatchPolicyContext(navigationId, target, route, CurrentSnapshot);

            if (!await policy.CanMatchAsync(context, cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    private async ValueTask<RouteGuardResult> RunEnterGuardsAsync(
        Guid navigationId,
        NavigationTarget target,
        RouteDescriptor route,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        foreach (var guardType in route.EnterGuardTypes)
        {
            var guard = Resolve<IRouteEnterGuard>(guardType);
            var context = new RouteGuardContext(navigationId, target, route, CurrentSnapshot, parameters);
            var result = await guard.CanEnterAsync(context, cancellationToken);

            if (result.Status != RouteGuardResultStatus.Allow)
            {
                return result;
            }
        }

        return RouteGuardResult.Allow();
    }

    private async ValueTask<RouteGuardResult> RunLeaveGuardsAsync(
        Guid navigationId,
        NavigationTarget target,
        RouteDescriptor route,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        foreach (var guardType in route.LeaveGuardTypes)
        {
            var guard = Resolve<IRouteLeaveGuard>(guardType);
            var context = new RouteGuardContext(navigationId, target, route, CurrentSnapshot, parameters);
            var result = await guard.CanLeaveAsync(context, cancellationToken);

            if (result.Status != RouteGuardResultStatus.Allow)
            {
                return result;
            }
        }

        return RouteGuardResult.Allow();
    }

    private TService Resolve<TService>(Type serviceType)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        var service = _serviceResolver(serviceType);

        if (service is TService typedService)
        {
            return typedService;
        }

        throw new InvalidOperationException(
            $"Service resolver did not return an instance of '{typeof(TService).FullName}' for '{serviceType.FullName}'.");
    }

    private static NavigationResult MapGuardResult(
        Guid navigationId,
        NavigationTarget target,
        RouteGuardResult result)
    {
        return result.Status switch
        {
            RouteGuardResultStatus.Reject => NavigationResult.Rejected(
                navigationId,
                target,
                result.Code ?? "CITY-NAVIGATION-REJECTED",
                result.Message),
            RouteGuardResultStatus.Cancel => NavigationResult.Cancelled(navigationId, target, result.Message),
            RouteGuardResultStatus.Redirect when result.RedirectTarget is not null => NavigationResult.Redirected(navigationId, target, result.RedirectTarget),
            RouteGuardResultStatus.Failed => NavigationResult.Failed(
                navigationId,
                target,
                result.Code ?? "CITY-NAVIGATION-GUARD-FAILED",
                result.Message ?? "Route guard failed.",
                result.Exception),
            _ => NavigationResult.Failed(
                navigationId,
                target,
                "CITY-NAVIGATION-GUARD-INVALID-RESULT",
                $"Route guard returned unsupported status '{result.Status}'."),
        };
    }
}
