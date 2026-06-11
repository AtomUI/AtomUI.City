using System.Security.Claims;

namespace AtomUI.City.Security;

public interface ICurrentPrincipalAccessor
{
    ClaimsPrincipal Principal { get; }
}
