namespace AtomUI.City.Security;

public sealed class AuthorizationResult
{
    private AuthorizationResult(
        AuthorizationResultStatus status,
        SecurityFailureKind failureKind,
        string? failedRequirement,
        string? message,
        Exception? exception)
    {
        Status = status;
        FailureKind = failureKind;
        FailedRequirement = failedRequirement;
        Message = message;
        Exception = exception;
    }

    public AuthorizationResultStatus Status { get; }

    public SecurityFailureKind FailureKind { get; }

    public string? FailedRequirement { get; }

    public string? Message { get; }

    public Exception? Exception { get; }

    public bool Succeeded => Status == AuthorizationResultStatus.Allowed;

    public static AuthorizationResult Allowed()
    {
        return new AuthorizationResult(
            AuthorizationResultStatus.Allowed,
            SecurityFailureKind.None,
            failedRequirement: null,
            message: null,
            exception: null);
    }

    public static AuthorizationResult Challenge(string? message = null)
    {
        return new AuthorizationResult(
            AuthorizationResultStatus.Challenge,
            SecurityFailureKind.AuthenticationRequired,
            failedRequirement: "authenticated",
            message,
            exception: null);
    }

    public static AuthorizationResult Forbidden(
        string failedRequirement,
        string? message = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failedRequirement);

        return new AuthorizationResult(
            AuthorizationResultStatus.Forbidden,
            SecurityFailureKind.RequirementFailed,
            failedRequirement,
            message,
            exception: null);
    }

    public static AuthorizationResult Failed(
        SecurityFailureKind failureKind,
        string? failedRequirement = null,
        string? message = null,
        Exception? exception = null)
    {
        return new AuthorizationResult(
            AuthorizationResultStatus.Failed,
            failureKind,
            failedRequirement,
            message,
            exception);
    }

    public static AuthorizationResult Cancelled(string? message = null)
    {
        return new AuthorizationResult(
            AuthorizationResultStatus.Cancelled,
            SecurityFailureKind.Cancelled,
            failedRequirement: null,
            message,
            exception: null);
    }
}
