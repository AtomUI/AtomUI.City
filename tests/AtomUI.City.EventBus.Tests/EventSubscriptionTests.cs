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
    public async Task StopAsyncWaitsForInFlightHandlerToComplete()
    {
        var eventBus = new InMemoryEventBus();
        var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseHandler = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var subscription = eventBus.Subscribe<TestEvent>(
            async _ =>
            {
                handlerStarted.SetResult();
                await releaseHandler.Task;
            });

        var publication = eventBus.PublishAsync(new TestEvent("running")).AsTask();
        await handlerStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var stop = subscription.StopAsync().AsTask();
        await Task.Delay(100);

        Assert.False(stop.IsCompleted);

        releaseHandler.SetResult();

        await stop.WaitAsync(TimeSpan.FromSeconds(5));
        await publication.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(EventSubscriptionState.Disposed, subscription.State);
    }

    [Fact]
    public async Task SubscribeAcceptsTypedEventHandlerInstance()
    {
        var eventBus = new InMemoryEventBus();
        var handler = new RecordingEventHandler();

        eventBus.Subscribe<TestEvent>(handler);

        var result = await eventBus.PublishAsync(new TestEvent("handled"));

        Assert.True(result.Succeeded);
        Assert.Equal("handled", handler.Value);
    }

    [Fact]
    public async Task SubscribeAcceptsOwnedTypedEventHandlerInstance()
    {
        var eventBus = new InMemoryEventBus();
        var owner = LifecycleScope.CreateRoot(LifecycleScopeKind.Application, "app");
        var handler = new RecordingEventHandler();

        var subscription = eventBus.Subscribe(owner, handler);

        await owner.StopAsync();
        var result = await eventBus.PublishAsync(new TestEvent("ignored"));

        Assert.Equal(EventSubscriptionState.Disposed, subscription.State);
        Assert.True(result.Succeeded);
        Assert.Equal(0, result.DeliveredCount);
        Assert.Null(handler.Value);
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

    private sealed class RecordingEventHandler : IEventHandler<TestEvent>
    {
        public string? Value { get; private set; }

        public ValueTask HandleAsync(EventContext<TestEvent> context)
        {
            Value = context.Event.Value;

            return ValueTask.CompletedTask;
        }
    }
}
