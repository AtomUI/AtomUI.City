using System.Security.Claims;
using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class AuthenticationStateTests
{
    [Fact]
    public void AuthenticationStateStoreStartsAsUnknownWithAnonymousPrincipal()
    {
        var store = new AuthenticationStateStore();

        Assert.Equal(AuthenticationState.Unknown, store.Current.State);
        Assert.Equal(0, store.Current.Revision);
        Assert.False(store.Current.Principal.Identity?.IsAuthenticated);
        Assert.Same(store.Current.Principal, ((ICurrentPrincipalAccessor)store).Principal);
    }

    [Fact]
    public void SetAuthenticatedPublishesSnapshotWithIncrementedRevision()
    {
        var store = new AuthenticationStateStore();
        var principal = CreatePrincipal("42", "settings.read");
        AuthenticationStateChangedEventArgs? observed = null;
        store.StateChanged += (_, args) => observed = args;

        var snapshot = store.SetAuthenticated(principal, scheme: "Bearer");

        Assert.Equal(AuthenticationState.Authenticated, snapshot.State);
        Assert.Equal(1, snapshot.Revision);
        Assert.Equal("Bearer", snapshot.Scheme);
        Assert.Same(principal, snapshot.Principal);
        Assert.Same(snapshot, store.Current);
        Assert.NotNull(observed);
        Assert.Equal(AuthenticationState.Unknown, observed.Previous.State);
        Assert.Same(snapshot, observed.Current);
    }

    [Fact]
    public void SetSignedOutClearsPrincipalAndIncrementsRevision()
    {
        var store = new AuthenticationStateStore();
        store.SetAuthenticated(CreatePrincipal("42", "settings.read"), scheme: "Bearer");

        var snapshot = store.SetSignedOut();

        Assert.Equal(AuthenticationState.SignedOut, snapshot.State);
        Assert.Equal(2, snapshot.Revision);
        Assert.False(snapshot.Principal.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task DelegateAccessTokenProviderReturnsTokenForAuthenticatedPrincipal()
    {
        var provider = new DelegateAccessTokenProvider((context, cancellationToken) =>
        {
            Assert.Equal("api", context.ResourceName);
            Assert.False(cancellationToken.IsCancellationRequested);

            return ValueTask.FromResult(AccessTokenResult.Success("token-value", "Bearer"));
        });

        var result = await provider.GetTokenAsync(new AccessTokenRequest("api"));

        Assert.Equal(AccessTokenResultStatus.Success, result.Status);
        Assert.Equal("token-value", result.Token);
        Assert.Equal("Bearer", result.Scheme);
    }

    private static ClaimsPrincipal CreatePrincipal(string subject, string permission)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, subject),
                new Claim("permission", permission),
            ],
            authenticationType: "Test");

        return new ClaimsPrincipal(identity);
    }
}
