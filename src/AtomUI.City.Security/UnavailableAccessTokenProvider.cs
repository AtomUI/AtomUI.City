namespace AtomUI.City.Security;

public sealed class UnavailableAccessTokenProvider : IAccessTokenProvider
{
    public ValueTask<AccessTokenResult> GetTokenAsync(
        AccessTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return ValueTask.FromResult(
            cancellationToken.IsCancellationRequested
                ? AccessTokenResult.Cancelled()
                : AccessTokenResult.Unavailable("No access token provider is configured."));
    }
}
