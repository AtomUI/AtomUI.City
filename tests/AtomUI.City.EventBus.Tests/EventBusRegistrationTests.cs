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
}
