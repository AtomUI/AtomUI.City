using AtomUI.City.Security;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Security.Tests;

public sealed class SecurityRegistrationTests
{
    [Fact]
    public void AddSecurityRegistersCoreServices()
    {
        var services = new ServiceCollection();

        services.AddSecurity();

        using var serviceProvider = services.BuildServiceProvider();
        var stateProvider = serviceProvider.GetRequiredService<IAuthenticationStateProvider>();
        var principalAccessor = serviceProvider.GetRequiredService<ICurrentPrincipalAccessor>();
        var registry = serviceProvider.GetRequiredService<IPermissionRegistry>();
        var policyProvider = serviceProvider.GetRequiredService<IAuthorizationPolicyProvider>();
        var permissionChecker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var evaluator = serviceProvider.GetRequiredService<IAuthorizationEvaluator>();
        var tokenProvider = serviceProvider.GetRequiredService<IAccessTokenProvider>();

        Assert.Same(stateProvider, principalAccessor);
        Assert.IsType<PermissionRegistry>(registry);
        Assert.IsType<InMemoryAuthorizationPolicyProvider>(policyProvider);
        Assert.IsType<PermissionChecker>(permissionChecker);
        Assert.IsType<AuthorizationEvaluator>(evaluator);
        Assert.IsType<UnavailableAccessTokenProvider>(tokenProvider);
    }
}
