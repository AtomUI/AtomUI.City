using System.Security.Claims;

namespace AtomUI.City.Security;

public sealed class PermissionChecker : IPermissionChecker
{
    private readonly IAuthorizationEvaluator _authorizationEvaluator;
    private readonly ICurrentPrincipalAccessor? _principalAccessor;

    public PermissionChecker(
        IPermissionRegistry permissions,
        ICurrentPrincipalAccessor? principalAccessor = null)
        : this(new AuthorizationEvaluator(permissions), principalAccessor)
    {
    }

    public PermissionChecker(
        IAuthorizationEvaluator authorizationEvaluator,
        ICurrentPrincipalAccessor? principalAccessor = null)
    {
        ArgumentNullException.ThrowIfNull(authorizationEvaluator);

        _authorizationEvaluator = authorizationEvaluator;
        _principalAccessor = principalAccessor;
    }

    public ValueTask<AuthorizationResult> CheckAsync(
        ClaimsPrincipal? principal,
        string permissionName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionName);

        return _authorizationEvaluator.EvaluateAsync(
            new AuthorizationRequest(
                principal,
                AuthorizationPolicy.RequirePermission($"Permission:{permissionName}", permissionName)),
            cancellationToken);
    }

    public ValueTask<AuthorizationResult> CheckCurrentAsync(
        string permissionName,
        CancellationToken cancellationToken = default)
    {
        if (_principalAccessor is null)
        {
            return ValueTask.FromResult(
                AuthorizationResult.Failed(
                    SecurityFailureKind.EvaluatorFailed,
                    message: "No current principal accessor is configured."));
        }

        return CheckAsync(
            _principalAccessor.Principal,
            permissionName,
            cancellationToken);
    }
}
