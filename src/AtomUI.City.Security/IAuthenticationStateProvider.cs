namespace AtomUI.City.Security;

public interface IAuthenticationStateProvider
{
    AuthenticationStateSnapshot Current { get; }

    event EventHandler<AuthenticationStateChangedEventArgs>? StateChanged;
}
