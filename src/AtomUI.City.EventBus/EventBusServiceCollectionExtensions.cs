using AtomUI.City.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.EventBus;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IEventContractRegistry, InMemoryEventContractRegistry>();
        services.TryAddSingleton(serviceProvider => new InMemoryEventBus(
            serviceProvider.GetRequiredService<IEventContractRegistry>(),
            serviceProvider.GetService<IHostDiagnostics>()));
        services.TryAddSingleton<IEventBus>(
            serviceProvider => serviceProvider.GetRequiredService<InMemoryEventBus>());
        services.TryAddSingleton<IEventPublisher>(
            serviceProvider => serviceProvider.GetRequiredService<IEventBus>());
        services.TryAddSingleton<IEventSubscriber>(
            serviceProvider => serviceProvider.GetRequiredService<IEventBus>());

        return services;
    }
}
