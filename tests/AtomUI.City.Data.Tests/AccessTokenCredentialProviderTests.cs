using AtomUI.City.Data;
using AtomUI.City.Security;

namespace AtomUI.City.Data.Tests;

public sealed class AccessTokenCredentialProviderTests
{
    [Fact]
    public async Task AnonymousAuthenticationReturnsNoneWithoutRequestingToken()
    {
        var requests = 0;
        var provider = new AccessTokenCredentialProvider(
            new DelegateAccessTokenProvider((_, _) =>
            {
                requests++;
                return ValueTask.FromResult(AccessTokenResult.Success("token", "Bearer"));
            }));

        var result = await provider.GetCredentialAsync(
            new DataAuthenticationContext(
                "catalog",
                "public-items",
                DataAuthenticationOptions.Anonymous));

        Assert.Equal(DataCredentialResultStatus.None, result.Status);
        Assert.Equal(0, requests);
    }

    [Fact]
    public async Task BearerAuthenticationMapsSuccessfulTokenToCredential()
    {
        AccessTokenRequest? observedRequest = null;
        var provider = new AccessTokenCredentialProvider(
            new DelegateAccessTokenProvider((request, _) =>
            {
                observedRequest = request;
                return ValueTask.FromResult(AccessTokenResult.Success("access-token", "Bearer"));
            }));

        var result = await provider.GetCredentialAsync(
            new DataAuthenticationContext(
                "catalog",
                "secure-items",
                DataAuthenticationOptions.Bearer()));

        Assert.Equal(DataCredentialResultStatus.Success, result.Status);
        Assert.Equal("Bearer", result.Credential?.Scheme);
        Assert.Equal("access-token", result.Credential?.Parameter);
        Assert.NotNull(observedRequest);
        Assert.Equal("catalog", observedRequest.ResourceName);
        Assert.Equal("secure-items", observedRequest.OperationName);
        Assert.Equal("Bearer", observedRequest.Scheme);
    }

    [Theory]
    [InlineData(AccessTokenResultStatus.None, DataCredentialResultStatus.None)]
    [InlineData(AccessTokenResultStatus.Required, DataCredentialResultStatus.Required)]
    [InlineData(AccessTokenResultStatus.Expired, DataCredentialResultStatus.Expired)]
    [InlineData(AccessTokenResultStatus.Unavailable, DataCredentialResultStatus.Unavailable)]
    [InlineData(AccessTokenResultStatus.Cancelled, DataCredentialResultStatus.Cancelled)]
    public async Task TokenResultStatusMapsToCredentialStatus(
        AccessTokenResultStatus tokenStatus,
        DataCredentialResultStatus expectedCredentialStatus)
    {
        var provider = new AccessTokenCredentialProvider(
            new DelegateAccessTokenProvider((_, _) =>
            {
                return ValueTask.FromResult(CreateTokenResult(tokenStatus));
            }));

        var result = await provider.GetCredentialAsync(
            new DataAuthenticationContext(
                "catalog",
                "secure-items",
                DataAuthenticationOptions.Bearer()));

        Assert.Equal(expectedCredentialStatus, result.Status);
        if (tokenStatus == AccessTokenResultStatus.None)
        {
            Assert.Null(result.Message);
        }
        else
        {
            Assert.Equal("token status", result.Message);
        }
    }

    [Fact]
    public async Task CancellationTokenFlowsToAccessTokenProvider()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var provider = new AccessTokenCredentialProvider(
            new DelegateAccessTokenProvider((_, cancellationToken) =>
            {
                Assert.True(cancellationToken.IsCancellationRequested);
                return ValueTask.FromResult(AccessTokenResult.Cancelled("cancelled"));
            }));

        var result = await provider.GetCredentialAsync(
            new DataAuthenticationContext(
                "catalog",
                "secure-items",
                DataAuthenticationOptions.Bearer()),
            cancellation.Token);

        Assert.Equal(DataCredentialResultStatus.Cancelled, result.Status);
    }

    private static AccessTokenResult CreateTokenResult(AccessTokenResultStatus status)
    {
        return status switch
        {
            AccessTokenResultStatus.None => AccessTokenResult.None(),
            AccessTokenResultStatus.Required => AccessTokenResult.Required("token status"),
            AccessTokenResultStatus.Expired => AccessTokenResult.Expired("token status"),
            AccessTokenResultStatus.Unavailable => AccessTokenResult.Unavailable("token status"),
            AccessTokenResultStatus.Cancelled => AccessTokenResult.Cancelled("token status"),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, message: null),
        };
    }
}
