namespace AtomUI.City.Security;

public sealed class AuthorizationResult
{
    private AuthorizationResult(
        AuthorizationResultStatus status,
        SecurityFailureKind failureKind,
        string? failedRequirement,
        string? message,
        string? messageKey,
        IReadOnlyList<object?>? messageArguments,
        Exception? exception)
    {
        Status = status;
        FailureKind = failureKind;
        FailedRequirement = failedRequirement;
        Message = message;
        MessageKey = messageKey;
        MessageArguments = messageArguments is null ? null : Array.AsReadOnly(messageArguments.ToArray());
        Exception = exception;
    }

    public AuthorizationResultStatus Status { get; }

    public SecurityFailureKind FailureKind { get; }

    public string? FailedRequirement { get; }

    public string? Message { get; }

    public string? MessageKey { get; }

    public IReadOnlyList<object?>? MessageArguments { get; }

    public Exception? Exception { get; }

    public bool Succeeded => Status == AuthorizationResultStatus.Allowed;

    public static AuthorizationResult Allowed()
    {
        return new AuthorizationResult(
            AuthorizationResultStatus.Allowed,
            SecurityFailureKind.None,
            failedRequirement: null,
            message: null,
            messageKey: null,
            messageArguments: null,
            exception: null);
    }

    public static AuthorizationResult Challenge(
        string? message = null,
        string? messageKey = "Errors.AuthenticationRequired",
        IReadOnlyList<object?>? messageArguments = null)
    {
        return new AuthorizationResult(
            AuthorizationResultStatus.Challenge,
            SecurityFailureKind.AuthenticationRequired,
            failedRequirement: "authenticated",
            message,
            messageKey,
            messageArguments,
            exception: null);
    }

    public static AuthorizationResult Forbidden(
        string failedRequirement,
        string? message = null,
        string? messageKey = "Errors.AuthorizationForbidden",
        IReadOnlyList<object?>? messageArguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failedRequirement);

        return new AuthorizationResult(
            AuthorizationResultStatus.Forbidden,
            SecurityFailureKind.RequirementFailed,
            failedRequirement,
            message,
            messageKey,
            messageArguments ?? [failedRequirement],
            exception: null);
    }

    public static AuthorizationResult Failed(
        SecurityFailureKind failureKind,
        string? failedRequirement = null,
        string? message = null,
        string? messageKey = null,
        IReadOnlyList<object?>? messageArguments = null,
        Exception? exception = null)
    {
        return new AuthorizationResult(
            AuthorizationResultStatus.Failed,
            failureKind,
            failedRequirement,
            message,
            messageKey,
            messageArguments,
            exception);
    }

    public static AuthorizationResult Cancelled(
        string? message = null,
        string? messageKey = "Errors.Cancelled",
        IReadOnlyList<object?>? messageArguments = null)
    {
        return new AuthorizationResult(
            AuthorizationResultStatus.Cancelled,
            SecurityFailureKind.Cancelled,
            failedRequirement: null,
            message,
            messageKey,
            messageArguments,
            exception: null);
    }
}
