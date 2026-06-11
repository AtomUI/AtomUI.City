using System.Security.Claims;

namespace AtomUI.City.Security;

public sealed class AuthenticationStateSnapshot
{
    public AuthenticationStateSnapshot(
        AuthenticationState state,
        ClaimsPrincipal principal,
        long revision,
        string? scheme = null,
        DateTimeOffset? expiresAt = null,
        string? failureMessage = null)
    {
        ArgumentNullException.ThrowIfNull(principal);

        State = state;
        Principal = principal;
        Revision = revision;
        Scheme = scheme;
        ExpiresAt = expiresAt;
        FailureMessage = failureMessage;
    }

    public AuthenticationState State { get; }

    public ClaimsPrincipal Principal { get; }

    public long Revision { get; }

    public string? Scheme { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public string? FailureMessage { get; }
}
