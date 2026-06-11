using AtomUI.City.EventBus;
using AtomUI.City.Lifecycle;

namespace AtomUI.City.EventBus.Tests;

public sealed class EventSubscriptionTests
{
    [Fact]
    public async Task DisposedSubscriptionNoLongerReceivesEvents()
    {
        var eventBus = new InMemoryEventBus();
        var receivedCount = 0;
        var subscription = eventBus.Subscribe<TestEvent>(_ =>
        {
            receivedCount++;
            return ValueTask.CompletedTask;
        });

        subscription.Dispose();
        await eventBus.PublishAsync(new TestEvent("ignored"));

        Assert.Equal(EventSubscriptionState.Disposed, subscription.State);
        Assert.Equal(0, receivedCount);
    }

    [Fact]
    public async Task StopAsyncRemovesSubscriptionFromNewPublicationSnapshots()
    {
        var eventBus = new InMemoryEventBus();
        var receivedCount = 0;
        var subscription = eventBus.Subscribe<TestEvent>(_ =>
        {
            receivedCount++;
            return ValueTask.CompletedTask;
        });

        await subscription.StopAsync();
        var result = await eventBus.PublishAsync(new TestEvent("ignored"));

        Assert.Equal(EventSubscriptionState.Disposed, subscription.State);
        Assert.Equal(0, receivedCount);
        Assert.Equal(0, result.DeliveredCount);
    }

    [Fact]
    public async Task OwnerScopeCancellationDisposesSubscription()
    {
        var eventBus = new InMemoryEventBus();
        var owner = LifecycleScope.CreateRoot(LifecycleScopeKind.Application, "app");
        var receivedCount = 0;
        var subscription = eventBus.Subscribe<TestEvent>(
            owner,
            _ =>
            {
                receivedCount++;
                return ValueTask.CompletedTask;
            });

        await owner.StopAsync();
        await eventBus.PublishAsync(new TestEvent("ignored"));

        Assert.Equal(EventSubscriptionState.Disposed, subscription.State);
        Assert.Equal(0, receivedCount);
    }

    private sealed record TestEvent(string Value);
}
