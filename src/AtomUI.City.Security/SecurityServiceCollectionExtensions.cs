using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Security;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<AuthenticationStateStore>();
        services.TryAddSingleton<IAuthenticationStateProvider>(
            serviceProvider => serviceProvider.GetRequiredService<AuthenticationStateStore>());
        services.TryAddSingleton<ICurrentPrincipalAccessor>(
            serviceProvider => serviceProvider.GetRequiredService<AuthenticationStateStore>());
        services.TryAddSingleton<PermissionRegistry>();
        services.TryAddSingleton<IPermissionRegistry>(
            serviceProvider => serviceProvider.GetRequiredService<PermissionRegistry>());
        services.TryAddSingleton<InMemoryAuthorizationPolicyProvider>();
        services.TryAddSingleton<IAuthorizationPolicyProvider>(
            serviceProvider => serviceProvider.GetRequiredService<InMemoryAuthorizationPolicyProvider>());
        services.TryAddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
        services.TryAddSingleton<IPermissionChecker>(
            serviceProvider => new PermissionChecker(
                serviceProvider.GetRequiredService<IAuthorizationEvaluator>(),
                serviceProvider.GetRequiredService<ICurrentPrincipalAccessor>()));
        services.TryAddSingleton<IAccessTokenProvider, UnavailableAccessTokenProvider>();
        services.TryAddSingleton<InMemoryRouteAuthorizationPolicyProvider>();
        services.TryAddSingleton<IRouteAuthorizationPolicyProvider>(
            serviceProvider => serviceProvider.GetRequiredService<InMemoryRouteAuthorizationPolicyProvider>());
        services.TryAddSingleton<SecurityRouteGuard>();

        return services;
    }
}
