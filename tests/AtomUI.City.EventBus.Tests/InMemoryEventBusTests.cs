using AtomUI.City.EventBus;

namespace AtomUI.City.EventBus.Tests;

public sealed class InMemoryEventBusTests
{
    [Fact]
    public async Task PublishInvokesMatchingHandlers()
    {
        var eventBus = new InMemoryEventBus();
        var received = string.Empty;

        eventBus.Subscribe<TestEvent>((eventData, _) =>
        {
            received = eventData.Value;
            return ValueTask.CompletedTask;
        });

        await eventBus.PublishAsync(new TestEvent("published"));

        Assert.Equal("published", received);
    }

    [Fact]
    public async Task DisposedSubscriptionNoLongerReceivesEvents()
    {
        var eventBus = new InMemoryEventBus();
        var receivedCount = 0;
        var subscription = eventBus.Subscribe<TestEvent>((_, _) =>
        {
            receivedCount++;
            return ValueTask.CompletedTask;
        });

        subscription.Dispose();
        await eventBus.PublishAsync(new TestEvent("ignored"));

        Assert.Equal(0, receivedCount);
    }

    private sealed record TestEvent(string Value);
}
