using System.Security.Claims;

namespace AtomUI.City.Security;

public sealed class AuthorizationEvaluator : IAuthorizationEvaluator
{
    private const string PermissionClaimType = "permission";

    private readonly IPermissionRegistry _permissions;

    public AuthorizationEvaluator(IPermissionRegistry permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        _permissions = permissions;
    }

    public ValueTask<AuthorizationResult> EvaluateAsync(
        AuthorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(AuthorizationResult.Cancelled());
        }

        try
        {
            foreach (var requirement in request.Policy.Requirements)
            {
                var result = EvaluateRequirement(request.Principal, requirement);
                if (!result.Succeeded)
                {
                    return ValueTask.FromResult(result);
                }
            }

            return ValueTask.FromResult(AuthorizationResult.Allowed());
        }
        catch (OperationCanceledException)
        {
            return ValueTask.FromResult(AuthorizationResult.Cancelled());
        }
        catch (Exception exception)
        {
            return ValueTask.FromResult(
                AuthorizationResult.Failed(
                    SecurityFailureKind.EvaluatorFailed,
                    message: exception.Message,
                    exception: exception));
        }
    }

    private AuthorizationResult EvaluateRequirement(
        ClaimsPrincipal principal,
        AuthorizationRequirement requirement)
    {
        return requirement.Kind switch
        {
            AuthorizationRequirementKind.Authenticated => EvaluateAuthenticated(principal),
            AuthorizationRequirementKind.Permission => EvaluatePermission(principal, requirement),
            AuthorizationRequirementKind.Claim => EvaluateClaim(principal, requirement),
            AuthorizationRequirementKind.Role => EvaluateRole(principal, requirement),
            _ => AuthorizationResult.Failed(
                SecurityFailureKind.EvaluatorFailed,
                requirement.Name,
                "Unsupported authorization requirement."),
        };
    }

    private static AuthorizationResult EvaluateAuthenticated(ClaimsPrincipal principal)
    {
        return IsAuthenticated(principal)
            ? AuthorizationResult.Allowed()
            : AuthorizationResult.Challenge("Authentication is required.");
    }

    private AuthorizationResult EvaluatePermission(
        ClaimsPrincipal principal,
        AuthorizationRequirement requirement)
    {
        if (!_permissions.Contains(requirement.Name))
        {
            return AuthorizationResult.Failed(
                SecurityFailureKind.PermissionNotFound,
                requirement.Name,
                $"Permission '{requirement.Name}' is not registered.");
        }

        if (!IsAuthenticated(principal))
        {
            return AuthorizationResult.Challenge("Authentication is required.");
        }

        return principal.HasClaim(PermissionClaimType, requirement.Name)
            ? AuthorizationResult.Allowed()
            : AuthorizationResult.Forbidden(
                requirement.Name,
                $"Permission '{requirement.Name}' is required.");
    }

    private static AuthorizationResult EvaluateClaim(
        ClaimsPrincipal principal,
        AuthorizationRequirement requirement)
    {
        if (!IsAuthenticated(principal))
        {
            return AuthorizationResult.Challenge("Authentication is required.");
        }

        var satisfied = requirement.Value is null
            ? principal.HasClaim(claim => claim.Type == requirement.Name)
            : principal.HasClaim(requirement.Name, requirement.Value);

        return satisfied
            ? AuthorizationResult.Allowed()
            : AuthorizationResult.Forbidden(
                requirement.Name,
                $"Claim '{requirement.Name}' is required.");
    }

    private static AuthorizationResult EvaluateRole(
        ClaimsPrincipal principal,
        AuthorizationRequirement requirement)
    {
        if (!IsAuthenticated(principal))
        {
            return AuthorizationResult.Challenge("Authentication is required.");
        }

        return principal.IsInRole(requirement.Name)
            || principal.HasClaim(ClaimTypes.Role, requirement.Name)
            ? AuthorizationResult.Allowed()
            : AuthorizationResult.Forbidden(
                requirement.Name,
                $"Role '{requirement.Name}' is required.");
    }

    private static bool IsAuthenticated(ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated == true;
    }
}
