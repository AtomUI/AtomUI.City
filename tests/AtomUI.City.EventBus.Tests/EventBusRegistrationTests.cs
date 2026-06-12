using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.EventBus.Tests;

public sealed class EventBusRegistrationTests
{
    [Fact]
    public async Task ServiceCollectionRegistersEventBusContracts()
    {
        var services = new ServiceCollection();

        services.AddEventBus();

        await using var serviceProvider = services.BuildServiceProvider();
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();
        var publisher = serviceProvider.GetRequiredService<IEventPublisher>();
        var subscriber = serviceProvider.GetRequiredService<IEventSubscriber>();
        var registry = serviceProvider.GetRequiredService<IEventContractRegistry>();

        Assert.Same(eventBus, publisher);
        Assert.Same(eventBus, subscriber);
        Assert.IsType<InMemoryEventBus>(eventBus);
        Assert.IsType<InMemoryEventContractRegistry>(registry);
    }

    [Fact]
    public async Task ServiceCollectionRegistersEventContractDescriptor()
    {
        var services = new ServiceCollection();
        var contractId = new EventContractId("atomui.city.tests.registration.v1");

        services.AddEventContract<RegisteredEvent>(contractId);

        await using var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<IEventContractRegistry>();

        var descriptor = registry.GetOrCreate<RegisteredEvent>();

        Assert.Equal(contractId, descriptor.ContractId);
        Assert.Equal(typeof(RegisteredEvent), descriptor.EventType);
    }

    private sealed record RegisteredEvent(string Value);
}
