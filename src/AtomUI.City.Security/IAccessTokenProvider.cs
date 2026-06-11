namespace AtomUI.City.Security;

public interface IAccessTokenProvider
{
    ValueTask<AccessTokenResult> GetTokenAsync(
        AccessTokenRequest request,
        CancellationToken cancellationToken = default);
}
