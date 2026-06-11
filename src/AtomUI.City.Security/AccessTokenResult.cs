namespace AtomUI.City.Security;

public sealed class AccessTokenResult
{
    private AccessTokenResult(
        AccessTokenResultStatus status,
        string? token,
        string? scheme,
        DateTimeOffset? expiresAt,
        string? message)
    {
        Status = status;
        Token = token;
        Scheme = scheme;
        ExpiresAt = expiresAt;
        Message = message;
    }

    public AccessTokenResultStatus Status { get; }

    public string? Token { get; }

    public string? Scheme { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public string? Message { get; }

    public bool Succeeded => Status == AccessTokenResultStatus.Success;

    public static AccessTokenResult Success(
        string token,
        string scheme,
        DateTimeOffset? expiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentException.ThrowIfNullOrWhiteSpace(scheme);

        return new AccessTokenResult(
            AccessTokenResultStatus.Success,
            token,
            scheme,
            expiresAt,
            message: null);
    }

    public static AccessTokenResult None()
    {
        return new AccessTokenResult(
            AccessTokenResultStatus.None,
            token: null,
            scheme: null,
            expiresAt: null,
            message: null);
    }

    public static AccessTokenResult Required(string? message = null)
    {
        return new AccessTokenResult(
            AccessTokenResultStatus.Required,
            token: null,
            scheme: null,
            expiresAt: null,
            message);
    }

    public static AccessTokenResult Expired(string? message = null)
    {
        return new AccessTokenResult(
            AccessTokenResultStatus.Expired,
            token: null,
            scheme: null,
            expiresAt: null,
            message);
    }

    public static AccessTokenResult Unavailable(string? message = null)
    {
        return new AccessTokenResult(
            AccessTokenResultStatus.Unavailable,
            token: null,
            scheme: null,
            expiresAt: null,
            message);
    }

    public static AccessTokenResult Cancelled(string? message = null)
    {
        return new AccessTokenResult(
            AccessTokenResultStatus.Cancelled,
            token: null,
            scheme: null,
            expiresAt: null,
            message);
    }
}
