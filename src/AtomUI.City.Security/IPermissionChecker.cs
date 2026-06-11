using System.Security.Claims;

namespace AtomUI.City.Security;

public interface IPermissionChecker
{
    ValueTask<AuthorizationResult> CheckAsync(
        ClaimsPrincipal? principal,
        string permissionName,
        CancellationToken cancellationToken = default);

    ValueTask<AuthorizationResult> CheckCurrentAsync(
        string permissionName,
        CancellationToken cancellationToken = default);
}
