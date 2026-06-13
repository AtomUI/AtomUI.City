namespace AtomUI.City.Security;

public sealed class AuthorizationPolicy
{
    public AuthorizationPolicy(
        string name,
        IReadOnlyCollection<AuthorizationRequirement> requirements,
        string? contributionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(requirements);

        Name = name;
        Requirements = Array.AsReadOnly(requirements.ToArray());
        ContributionId = contributionId;
    }

    public string Name { get; }

    public IReadOnlyList<AuthorizationRequirement> Requirements { get; }

    public string? ContributionId { get; }

    public static AuthorizationPolicy RequireAuthenticated(string name)
    {
        return new AuthorizationPolicy(
            name,
            [AuthorizationRequirement.RequireAuthenticated()]);
    }

    public static AuthorizationPolicy RequirePermission(
        string name,
        string permissionName,
        string? contributionId = null)
    {
        return new AuthorizationPolicy(
            name,
            [
                AuthorizationRequirement.RequireAuthenticated(),
                AuthorizationRequirement.RequirePermission(permissionName),
            ],
            contributionId);
    }
}
