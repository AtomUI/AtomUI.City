using System.Security.Claims;

namespace AtomUI.City.Security;

public sealed class AuthorizationRequest
{
    public AuthorizationRequest(
        ClaimsPrincipal? principal,
        AuthorizationPolicy policy,
        string? resourceName = null,
        string? contributionId = null)
    {
        ArgumentNullException.ThrowIfNull(policy);

        Principal = principal ?? SecurityPrincipals.Anonymous;
        Policy = policy;
        ResourceName = resourceName;
        ContributionId = contributionId;
    }

    public ClaimsPrincipal Principal { get; }

    public AuthorizationPolicy Policy { get; }

    public string? ResourceName { get; }

    public string? ContributionId { get; }
}
