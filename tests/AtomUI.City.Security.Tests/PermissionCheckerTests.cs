using System.Security.Claims;
using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class PermissionCheckerTests
{
    [Fact]
    public async Task CheckAsyncAllowsPrincipalWithRegisteredPermissionClaim()
    {
        var registry = new PermissionRegistry();
        registry.Add(new PermissionDescriptor("settings.read"));
        var checker = new PermissionChecker(registry);
        var principal = CreatePrincipal(["settings.read"]);

        var result = await checker.CheckAsync(principal, "settings.read");

        Assert.Equal(AuthorizationResultStatus.Allowed, result.Status);
    }

    [Fact]
    public async Task CheckAsyncChallengesAnonymousPrincipal()
    {
        var registry = new PermissionRegistry();
        registry.Add(new PermissionDescriptor("settings.read"));
        var checker = new PermissionChecker(registry);

        var result = await checker.CheckAsync(new ClaimsPrincipal(new ClaimsIdentity()), "settings.read");

        Assert.Equal(AuthorizationResultStatus.Challenge, result.Status);
    }

    [Fact]
    public async Task CheckCurrentAsyncUsesCurrentPrincipalAccessor()
    {
        var registry = new PermissionRegistry();
        registry.Add(new PermissionDescriptor("settings.read"));
        var store = new AuthenticationStateStore();
        store.SetAuthenticated(CreatePrincipal(["settings.read"]));
        var checker = new PermissionChecker(registry, store);

        var result = await checker.CheckCurrentAsync("settings.read");

        Assert.Equal(AuthorizationResultStatus.Allowed, result.Status);
    }

    private static ClaimsPrincipal CreatePrincipal(IReadOnlyCollection<string> permissions)
    {
        var identity = new ClaimsIdentity(authenticationType: "Test");

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim("permission", permission));
        }

        return new ClaimsPrincipal(identity);
    }
}
