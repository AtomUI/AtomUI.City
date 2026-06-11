namespace AtomUI.City.Security;

public sealed class DelegateAccessTokenProvider : IAccessTokenProvider
{
    private readonly Func<AccessTokenRequest, CancellationToken, ValueTask<AccessTokenResult>> _provider;

    public DelegateAccessTokenProvider(
        Func<AccessTokenRequest, CancellationToken, ValueTask<AccessTokenResult>> provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        _provider = provider;
    }

    public ValueTask<AccessTokenResult> GetTokenAsync(
        AccessTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _provider(request, cancellationToken);
    }
}
