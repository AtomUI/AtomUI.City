using AtomUI.City.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AtomUI.City.EventBus;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IEventContractRegistry>(serviceProvider =>
        {
            var registry = new InMemoryEventContractRegistry();

            foreach (var descriptor in serviceProvider.GetServices<EventContractDescriptor>())
            {
                registry.Register(descriptor);
            }

            return registry;
        });
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

    public static IServiceCollection AddEventContract<TEvent>(
        this IServiceCollection services,
        EventContractId contractId)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddEventBus();
        services.AddSingleton(EventContractDescriptor.Shared<TEvent>(contractId, typeof(TEvent).Assembly));

        return services;
    }
}
