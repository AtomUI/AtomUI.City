using AtomUI.City.EventBus;

namespace AtomUI.City.EventBus.Tests;

public sealed class EventPublicationTests
{
    [Fact]
    public async Task PublishAsyncInvokesMatchingHandlersWithEventContext()
    {
        var eventBus = new InMemoryEventBus();
        EventContext<TestEvent>? observedContext = null;

        eventBus.Subscribe<TestEvent>(context =>
        {
            observedContext = context;
            return ValueTask.CompletedTask;
        });

        var result = await eventBus.PublishAsync(new TestEvent("published"));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.DeliveredCount);
        Assert.NotEqual(Guid.Empty, result.EventId);
        Assert.NotNull(observedContext);
        Assert.Equal("published", observedContext.Event.Value);
        Assert.Equal(result.EventId, observedContext.EventId);
        Assert.Equal(result.ContractId, observedContext.ContractId);
        Assert.Equal(0, observedContext.PublishDepth);
    }

    [Fact]
    public async Task PublishAsyncSupportsSyncAndAsyncHandlers()
    {
        var eventBus = new InMemoryEventBus();
        var received = new List<string>();

        eventBus.Subscribe<TestEvent>(context => received.Add("sync:" + context.Event.Value));
        eventBus.Subscribe<TestEvent>(async context =>
        {
            await Task.Yield();
            received.Add("async:" + context.Event.Value);
        });

        var result = await eventBus.PublishAsync(new TestEvent("value"));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.DeliveredCount);
        Assert.Contains("sync:value", received);
        Assert.Contains("async:value", received);
    }

    [Fact]
    public async Task PublishAsyncUsesRegisteredContractId()
    {
        var contracts = new InMemoryEventContractRegistry();
        var contractId = new EventContractId("atomui.city.tests.event.v1");
        contracts.Register(EventContractDescriptor.Shared<TestEvent>(contractId, typeof(TestEvent).Assembly));
        var eventBus = new InMemoryEventBus(contractRegistry: contracts);
        EventContractId observedContractId = default;

        eventBus.Subscribe<TestEvent>(context =>
        {
            observedContractId = context.ContractId;
            return ValueTask.CompletedTask;
        });

        var result = await eventBus.PublishAsync(new TestEvent("published"));

        Assert.Equal(contractId, result.ContractId);
        Assert.Equal(contractId, observedContractId);
    }

    [Fact]
    public void PublishResultDeliveriesRejectExternalListMutation()
    {
        var delivery = new EventDeliveryResult(
            EventSubscriptionId.New(),
            EventDispatchPolicy.Serialized,
            Succeeded: true);
        var replacement = new EventDeliveryResult(
            EventSubscriptionId.New(),
            EventDispatchPolicy.Background,
            Succeeded: false,
            ErrorMessage: "replaced");
        var result = new EventPublishResult(
            Guid.NewGuid(),
            new EventContractId("atomui.city.tests.event.v1"),
            [delivery]);
        var list = Assert.IsAssignableFrom<IList<EventDeliveryResult>>(result.Deliveries);

        Assert.Throws<NotSupportedException>(() => list[0] = replacement);
        Assert.Equal(delivery.SubscriptionId, result.Deliveries[0].SubscriptionId);
    }

    [Fact]
    public async Task PostAsyncReturnsAcceptedEventIdUsedByDelivery()
    {
        var eventBus = new InMemoryEventBus();
        var observedEventId = new TaskCompletionSource<Guid>();

        eventBus.Subscribe<TestEvent>(context =>
        {
            observedEventId.SetResult(context.EventId);

            return ValueTask.CompletedTask;
        });

        var result = await eventBus.PostAsync(new TestEvent("posted"));

        Assert.True(result.Accepted);
        Assert.Equal(result.EventId, await observedEventId.Task.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task PostAsyncRejectsAlreadyCanceledPublication()
    {
        var eventBus = new InMemoryEventBus();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await eventBus.PostAsync(
            new TestEvent("posted"),
            cancellationToken: cancellation.Token);

        Assert.False(result.Accepted);
        Assert.NotEqual(Guid.Empty, result.EventId);
        Assert.False(string.IsNullOrWhiteSpace(result.RejectionReason));
    }

    private sealed record TestEvent(string Value);
}
