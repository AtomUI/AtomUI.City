namespace AtomUI.City.Security;

public sealed class AuthorizationRequirement
{
    private AuthorizationRequirement(
        AuthorizationRequirementKind kind,
        string name,
        string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Kind = kind;
        Name = name;
        Value = value;
    }

    public AuthorizationRequirementKind Kind { get; }

    public string Name { get; }

    public string? Value { get; }

    public static AuthorizationRequirement RequireAuthenticated()
    {
        return new AuthorizationRequirement(
            AuthorizationRequirementKind.Authenticated,
            "authenticated",
            value: null);
    }

    public static AuthorizationRequirement RequirePermission(string permissionName)
    {
        return new AuthorizationRequirement(
            AuthorizationRequirementKind.Permission,
            permissionName,
            value: null);
    }

    public static AuthorizationRequirement RequireClaim(string claimType, string? claimValue = null)
    {
        return new AuthorizationRequirement(
            AuthorizationRequirementKind.Claim,
            claimType,
            claimValue);
    }

    public static AuthorizationRequirement RequireRole(string roleName)
    {
        return new AuthorizationRequirement(
            AuthorizationRequirementKind.Role,
            roleName,
            value: null);
    }
}
