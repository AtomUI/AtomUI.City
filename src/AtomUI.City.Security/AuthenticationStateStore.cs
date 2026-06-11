using System.Security.Claims;

namespace AtomUI.City.Security;

public sealed class AuthenticationStateStore :
    IAuthenticationStateProvider,
    ICurrentPrincipalAccessor
{
    private readonly object _syncRoot = new();
    private AuthenticationStateSnapshot _current = new(
        AuthenticationState.Unknown,
        SecurityPrincipals.Anonymous,
        revision: 0);

    public event EventHandler<AuthenticationStateChangedEventArgs>? StateChanged;

    public AuthenticationStateSnapshot Current
    {
        get
        {
            lock (_syncRoot)
            {
                return _current;
            }
        }
    }

    public ClaimsPrincipal Principal => Current.Principal;

    public AuthenticationStateSnapshot SetAnonymous()
    {
        return SetCore(AuthenticationState.Anonymous, SecurityPrincipals.Anonymous);
    }

    public AuthenticationStateSnapshot SetAuthenticating(ClaimsPrincipal? principal = null, string? scheme = null)
    {
        return SetCore(AuthenticationState.Authenticating, principal ?? SecurityPrincipals.Anonymous, scheme);
    }

    public AuthenticationStateSnapshot SetAuthenticated(
        ClaimsPrincipal principal,
        string? scheme = null,
        DateTimeOffset? expiresAt = null)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return SetCore(AuthenticationState.Authenticated, principal, scheme, expiresAt);
    }

    public AuthenticationStateSnapshot SetRefreshing(ClaimsPrincipal principal, string? scheme = null)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return SetCore(AuthenticationState.Refreshing, principal, scheme);
    }

    public AuthenticationStateSnapshot SetExpired(ClaimsPrincipal principal, string? scheme = null)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return SetCore(AuthenticationState.Expired, principal, scheme);
    }

    public AuthenticationStateSnapshot SetSignedOut()
    {
        return SetCore(AuthenticationState.SignedOut, SecurityPrincipals.Anonymous);
    }

    public AuthenticationStateSnapshot SetFailed(string failureMessage, ClaimsPrincipal? principal = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(failureMessage);

        return SetCore(
            AuthenticationState.Failed,
            principal ?? SecurityPrincipals.Anonymous,
            scheme: null,
            expiresAt: null,
            failureMessage);
    }

    private AuthenticationStateSnapshot SetCore(
        AuthenticationState state,
        ClaimsPrincipal principal,
        string? scheme = null,
        DateTimeOffset? expiresAt = null,
        string? failureMessage = null)
    {
        AuthenticationStateSnapshot previous;
        AuthenticationStateSnapshot current;

        lock (_syncRoot)
        {
            previous = _current;
            current = new AuthenticationStateSnapshot(
                state,
                principal,
                previous.Revision + 1,
                scheme,
                expiresAt,
                failureMessage);
            _current = current;
        }

        StateChanged?.Invoke(this, new AuthenticationStateChangedEventArgs(previous, current));

        return current;
    }
}
