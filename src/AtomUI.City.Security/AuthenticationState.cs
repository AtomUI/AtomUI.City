namespace AtomUI.City.Security;

public enum AuthenticationState
{
    Unknown,
    Anonymous,
    Authenticating,
    Authenticated,
    Refreshing,
    Expired,
    SignedOut,
    Failed,
}
