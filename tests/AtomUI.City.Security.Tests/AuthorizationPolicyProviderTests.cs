using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class AuthorizationPolicyProviderTests
{
    [Fact]
    public void AddStoresPolicyByName()
    {
        var provider = new InMemoryAuthorizationPolicyProvider();
        var policy = AuthorizationPolicy.RequireAuthenticated("SignedIn");

        var added = provider.Add(policy);

        Assert.True(added);
        Assert.Equal(1, provider.Revision);
        Assert.True(provider.TryGet("SignedIn", out var stored));
        Assert.Same(policy, stored);
    }

    [Fact]
    public void AddRejectsDuplicatePolicyWithoutChangingRevision()
    {
        var provider = new InMemoryAuthorizationPolicyProvider();
        provider.Add(AuthorizationPolicy.RequireAuthenticated("SignedIn"));

        var added = provider.Add(AuthorizationPolicy.RequireAuthenticated("SignedIn"));

        Assert.False(added);
        Assert.Equal(1, provider.Revision);
    }

    [Fact]
    public void RemoveByContributionRevokesMatchingPolicies()
    {
        var provider = new InMemoryAuthorizationPolicyProvider();
        provider.Add(AuthorizationPolicy.RequireAuthenticated("HostPolicy"));
        provider.Add(AuthorizationPolicy.RequirePermission("PluginPolicy", "plugin.sales.export", contributionId: "SalesPlugin"));

        var removed = provider.RemoveByContribution("SalesPlugin");

        Assert.Equal(1, removed);
        Assert.Equal(3, provider.Revision);
        Assert.True(provider.Contains("HostPolicy"));
        Assert.False(provider.Contains("PluginPolicy"));
    }
}
