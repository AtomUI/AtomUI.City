namespace AtomUI.City.Routing;

public sealed class NavigationResult
{
    private NavigationResult(
        Guid navigationId,
        NavigationResultStatus status,
        NavigationTarget target,
        RouteDescriptor? route,
        IReadOnlyDictionary<string, string> parameters,
        NavigationError? error)
    {
        NavigationId = navigationId;
        Status = status;
        Target = target;
        ActiveRoute = route;
        Parameters = RouteParameters.Copy(parameters);
        Error = error;
    }

    public Guid NavigationId { get; }

    public NavigationResultStatus Status { get; }

    public NavigationTarget Target { get; }

    public RouteDescriptor Route => ActiveRoute ?? throw new InvalidOperationException("Navigation did not produce an active route.");

    public RouteDescriptor? ActiveRoute { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }

    public NavigationError? Error { get; }

    public static NavigationResult Success(
        Guid navigationId,
        NavigationTarget target,
        RouteDescriptor route,
        IReadOnlyDictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(parameters);

        return new NavigationResult(
            navigationId,
            NavigationResultStatus.Success,
            target,
            route,
            parameters,
            error: null);
    }

    public static NavigationResult NotFound(
        Guid navigationId,
        NavigationTarget target,
        string message)
    {
        return Failure(
            navigationId,
            NavigationResultStatus.NotFound,
            target,
            "CITY-NAVIGATION-NOT-FOUND",
            message);
    }

    public static NavigationResult Rejected(
        Guid navigationId,
        NavigationTarget target,
        string code,
        string? message = null)
    {
        return Failure(
            navigationId,
            NavigationResultStatus.Rejected,
            target,
            code,
            message ?? "Navigation was rejected.");
    }

    public static NavigationResult Cancelled(
        Guid navigationId,
        NavigationTarget target,
        string? message = null)
    {
        return Failure(
            navigationId,
            NavigationResultStatus.Cancelled,
            target,
            "CITY-NAVIGATION-CANCELLED",
            message ?? "Navigation was cancelled.");
    }

    public static NavigationResult Failed(
        Guid navigationId,
        NavigationTarget target,
        string code,
        string message,
        Exception? exception = null)
    {
        return new NavigationResult(
            navigationId,
            NavigationResultStatus.Failed,
            target,
            route: null,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            new NavigationError(code, message, exception));
    }

    public static NavigationResult Redirected(
        Guid navigationId,
        NavigationTarget target,
        NavigationTarget redirectTarget)
    {
        ArgumentNullException.ThrowIfNull(redirectTarget);

        return new NavigationResult(
            navigationId,
            NavigationResultStatus.Redirected,
            target,
            route: null,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            new NavigationError("CITY-NAVIGATION-REDIRECTED", $"Navigation redirected to '{redirectTarget}'."));
    }

    private static NavigationResult Failure(
        Guid navigationId,
        NavigationResultStatus status,
        NavigationTarget target,
        string code,
        string message)
    {
        ArgumentNullException.ThrowIfNull(target);

        return new NavigationResult(
            navigationId,
            status,
            target,
            route: null,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            new NavigationError(code, message));
    }
}
