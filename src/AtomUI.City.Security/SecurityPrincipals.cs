using System.Security.Claims;

namespace AtomUI.City.Security;

internal static class SecurityPrincipals
{
    public static ClaimsPrincipal Anonymous { get; } = new(new ClaimsIdentity());
}
