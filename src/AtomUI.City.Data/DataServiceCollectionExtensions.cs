using AtomUI.City.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.Data;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddData(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IAccessTokenProvider, UnavailableAccessTokenProvider>();
        services.TryAddSingleton<IDataCredentialProvider, AccessTokenCredentialProvider>();
        services.TryAddSingleton<DataClientRegistry>();
        services.TryAddSingleton<IDataClientFactory>(
            serviceProvider => serviceProvider.GetRequiredService<DataClientRegistry>());
        services.TryAddSingleton<IDataRequestPipeline, DataRequestPipeline>();

        return services;
    }
}
