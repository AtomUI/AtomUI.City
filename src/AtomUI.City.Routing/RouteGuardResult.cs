namespace AtomUI.City.Routing;

public sealed class RouteGuardResult
{
    private RouteGuardResult(
        RouteGuardResultStatus status,
        string? code,
        string? message,
        NavigationTarget? redirectTarget,
        Exception? exception)
    {
        Status = status;
        Code = code;
        Message = message;
        RedirectTarget = redirectTarget;
        Exception = exception;
    }

    public RouteGuardResultStatus Status { get; }

    public string? Code { get; }

    public string? Message { get; }

    public NavigationTarget? RedirectTarget { get; }

    public Exception? Exception { get; }

    public static RouteGuardResult Allow()
    {
        return new RouteGuardResult(
            RouteGuardResultStatus.Allow,
            code: null,
            message: null,
            redirectTarget: null,
            exception: null);
    }

    public static RouteGuardResult Reject(string code, string? message = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return new RouteGuardResult(
            RouteGuardResultStatus.Reject,
            code,
            message,
            redirectTarget: null,
            exception: null);
    }

    public static RouteGuardResult Cancel(string? message = null)
    {
        return new RouteGuardResult(
            RouteGuardResultStatus.Cancel,
            code: null,
            message,
            redirectTarget: null,
            exception: null);
    }

    public static RouteGuardResult Redirect(NavigationTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        return new RouteGuardResult(
            RouteGuardResultStatus.Redirect,
            code: null,
            message: null,
            target,
            exception: null);
    }

    public static RouteGuardResult Failed(
        string code,
        string message,
        Exception? exception = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new RouteGuardResult(
            RouteGuardResultStatus.Failed,
            code,
            message,
            redirectTarget: null,
            exception);
    }
}
