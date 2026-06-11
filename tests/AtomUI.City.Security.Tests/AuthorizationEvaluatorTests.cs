using System.Security.Claims;
using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class AuthorizationEvaluatorTests
{
    [Fact]
    public async Task AuthenticatedPolicyChallengesAnonymousPrincipal()
    {
        var evaluator = CreateEvaluator();
        var request = new AuthorizationRequest(
            ClaimsPrincipal.Current,
            AuthorizationPolicy.RequireAuthenticated("SignedIn"));

        var result = await evaluator.EvaluateAsync(request);

        Assert.Equal(AuthorizationResultStatus.Challenge, result.Status);
        Assert.Equal(SecurityFailureKind.AuthenticationRequired, result.FailureKind);
        Assert.Equal("Errors.AuthenticationRequired", result.MessageKey);
    }

    [Fact]
    public async Task PermissionRequirementAllowsMatchingPermissionClaim()
    {
        var registry = new PermissionRegistry();
        registry.Add(new PermissionDescriptor("settings.read"));
        var evaluator = CreateEvaluator(registry);
        var principal = CreatePrincipal(
            permissions: ["settings.read"],
            claims: [],
            roles: []);
        var request = new AuthorizationRequest(
            principal,
            AuthorizationPolicy.RequirePermission("CanReadSettings", "settings.read"));

        var result = await evaluator.EvaluateAsync(request);

        Assert.Equal(AuthorizationResultStatus.Allowed, result.Status);
    }

    [Fact]
    public async Task PermissionRequirementForAuthenticatedPrincipalWithoutPermissionReturnsForbidden()
    {
        var registry = new PermissionRegistry();
        registry.Add(new PermissionDescriptor("settings.write"));
        var evaluator = CreateEvaluator(registry);
        var request = new AuthorizationRequest(
            CreatePrincipal(permissions: ["settings.read"], claims: [], roles: []),
            AuthorizationPolicy.RequirePermission("CanWriteSettings", "settings.write"));

        var result = await evaluator.EvaluateAsync(request);

        Assert.Equal(AuthorizationResultStatus.Forbidden, result.Status);
        Assert.Equal(SecurityFailureKind.RequirementFailed, result.FailureKind);
        Assert.Equal("settings.write", result.FailedRequirement);
        Assert.Equal("Errors.AuthorizationForbidden", result.MessageKey);
        Assert.Equal(["settings.write"], result.MessageArguments);
    }

    [Fact]
    public async Task UnknownPermissionReturnsFailedResult()
    {
        var evaluator = CreateEvaluator();
        var request = new AuthorizationRequest(
            CreatePrincipal(permissions: ["settings.read"], claims: [], roles: []),
            AuthorizationPolicy.RequirePermission("CanReadSettings", "settings.read"));

        var result = await evaluator.EvaluateAsync(request);

        Assert.Equal(AuthorizationResultStatus.Failed, result.Status);
        Assert.Equal(SecurityFailureKind.PermissionNotFound, result.FailureKind);
    }

    [Fact]
    public async Task ClaimAndRoleRequirementsAreEvaluated()
    {
        var evaluator = CreateEvaluator();
        var principal = CreatePrincipal(
            permissions: [],
            claims: [new Claim("department", "finance")],
            roles: ["admin"]);
        var policy = new AuthorizationPolicy(
            "CanAdminFinance",
            [
                AuthorizationRequirement.RequireClaim("department", "finance"),
                AuthorizationRequirement.RequireRole("admin"),
            ]);

        var result = await evaluator.EvaluateAsync(new AuthorizationRequest(principal, policy));

        Assert.Equal(AuthorizationResultStatus.Allowed, result.Status);
    }

    [Fact]
    public async Task CancelledEvaluationReturnsCancelledResult()
    {
        var evaluator = CreateEvaluator();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await evaluator.EvaluateAsync(
            new AuthorizationRequest(ClaimsPrincipal.Current, AuthorizationPolicy.RequireAuthenticated("SignedIn")),
            cancellation.Token);

        Assert.Equal(AuthorizationResultStatus.Cancelled, result.Status);
    }

    private static AuthorizationEvaluator CreateEvaluator(PermissionRegistry? registry = null)
    {
        return new AuthorizationEvaluator(registry ?? new PermissionRegistry());
    }

    private static ClaimsPrincipal CreatePrincipal(
        IReadOnlyCollection<string> permissions,
        IReadOnlyCollection<Claim> claims,
        IReadOnlyCollection<string> roles)
    {
        var identity = new ClaimsIdentity(authenticationType: "Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "42"));

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim("permission", permission));
        }

        foreach (var claim in claims)
        {
            identity.AddClaim(claim);
        }

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(identity);
    }
}
