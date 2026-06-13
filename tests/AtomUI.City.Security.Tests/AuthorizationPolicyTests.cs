using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class AuthorizationPolicyTests
{
    [Fact]
    public void RequirementsRejectExternalListMutation()
    {
        var policy = new AuthorizationPolicy(
            "CanManageSettings",
            [
                AuthorizationRequirement.RequireAuthenticated(),
                AuthorizationRequirement.RequirePermission("settings.manage"),
            ]);
        var requirements = Assert.IsAssignableFrom<IList<AuthorizationRequirement>>(policy.Requirements);

        Assert.Throws<NotSupportedException>(() => requirements[0] = AuthorizationRequirement.RequireRole("admin"));
        Assert.Equal(AuthorizationRequirementKind.Authenticated, policy.Requirements[0].Kind);
        Assert.Equal("settings.manage", policy.Requirements[1].Name);
    }
}
