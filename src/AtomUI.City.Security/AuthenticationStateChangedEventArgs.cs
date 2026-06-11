namespace AtomUI.City.Security;

public sealed class AuthenticationStateChangedEventArgs : EventArgs
{
    public AuthenticationStateChangedEventArgs(
        AuthenticationStateSnapshot previous,
        AuthenticationStateSnapshot current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        Previous = previous;
        Current = current;
    }

    public AuthenticationStateSnapshot Previous { get; }

    public AuthenticationStateSnapshot Current { get; }
}
