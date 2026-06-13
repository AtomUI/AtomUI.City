using AtomUI.City.Diagnostics;
using AtomUI.City.EventBus;

namespace AtomUI.City.EventBus.Tests;

public sealed class EventDiagnosticsTests
{
    [Fact]
    public async Task HandlerFailureIsReportedAndDoesNotStopIndependentHandlers()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var eventBus = new InMemoryEventBus(diagnostics: diagnostics);
        var independentHandlerCalled = false;

        eventBus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("boom"));
        eventBus.Subscribe<TestEvent>(_ =>
        {
            independentHandlerCalled = true;
            return ValueTask.CompletedTask;
        });

        var result = await eventBus.PublishAsync(new TestEvent("failure"));

        Assert.False(result.Succeeded);
        Assert.True(independentHandlerCalled);
        Assert.Equal(2, result.DeliveredCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Contains(result.Deliveries, delivery => !delivery.Succeeded);
        Assert.Contains(diagnostics.Records, record => record.Code == EventDiagnosticIds.EventDeliveryFailed);
    }

    [Fact]
    public async Task StopPublicationErrorPolicySkipsLaterHandlers()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var eventBus = new InMemoryEventBus(diagnostics: diagnostics);
        var laterHandlerCalled = false;

        eventBus.Subscribe<TestEvent>(
            _ => throw new InvalidOperationException("boom"),
            EventSubscriptionOptions.Serialized.WithErrorPolicy(EventErrorPolicy.StopPublication));
        eventBus.Subscribe<TestEvent>(_ =>
        {
            laterHandlerCalled = true;
            return ValueTask.CompletedTask;
        });

        var result = await eventBus.PublishAsync(new TestEvent("failure"));

        Assert.False(result.Succeeded);
        Assert.False(laterHandlerCalled);
        Assert.Equal(1, result.DeliveredCount);
        Assert.Equal(1, result.FailedCount);
        Assert.Contains(diagnostics.Records, record => record.Code == EventDiagnosticIds.EventDeliveryFailed);
    }

    [Fact]
    public async Task FailPublisherErrorPolicyPropagatesHandlerFailure()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var eventBus = new InMemoryEventBus(diagnostics: diagnostics);

        eventBus.Subscribe<TestEvent>(
            _ => throw new InvalidOperationException("boom"),
            EventSubscriptionOptions.Serialized.WithErrorPolicy(EventErrorPolicy.FailPublisher));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await eventBus.PublishAsync(new TestEvent("failure")));

        Assert.Equal("boom", exception.Message);
        Assert.Contains(diagnostics.Records, record => record.Code == EventDiagnosticIds.EventDeliveryFailed);
    }

    [Fact]
    public async Task HandlerCancellationIsTrackedSeparatelyFromFailure()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var eventBus = new InMemoryEventBus(diagnostics: diagnostics);
        using var cancellation = new CancellationTokenSource();

        eventBus.Subscribe<TestEvent>(context =>
        {
            cancellation.Cancel();
            context.CancellationToken.ThrowIfCancellationRequested();

            return ValueTask.CompletedTask;
        });

        var result = await eventBus.PublishAsync(
            new TestEvent("cancel"),
            cancellationToken: cancellation.Token);

        var delivery = Assert.Single(result.Deliveries);
        Assert.False(result.Succeeded);
        Assert.Equal(1, result.DeliveredCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Equal(1, result.CanceledCount);
        Assert.False(delivery.Succeeded);
        Assert.True(delivery.Canceled);
        Assert.Contains(diagnostics.Records, record => record.Code == EventDiagnosticIds.EventDeliveryCancelled);
        Assert.DoesNotContain(diagnostics.Records, record => record.Code == EventDiagnosticIds.EventDeliveryFailed);
    }

    [Fact]
    public async Task HandlerCancellationStopsLaterDeliveriesWithoutThrowing()
    {
        var eventBus = new InMemoryEventBus();
        using var cancellation = new CancellationTokenSource();
        var laterHandlerCalled = false;

        eventBus.Subscribe<TestEvent>(context =>
        {
            cancellation.Cancel();
            context.CancellationToken.ThrowIfCancellationRequested();

            return ValueTask.CompletedTask;
        });
        eventBus.Subscribe<TestEvent>(_ =>
        {
            laterHandlerCalled = true;

            return ValueTask.CompletedTask;
        });

        var result = await eventBus.PublishAsync(
            new TestEvent("cancel"),
            cancellationToken: cancellation.Token);

        Assert.False(result.Succeeded);
        Assert.False(laterHandlerCalled);
        Assert.Equal(1, result.DeliveredCount);
        Assert.Equal(1, result.CanceledCount);
        Assert.Equal(0, result.FailedCount);
    }

    [Fact]
    public async Task PostAsyncAcceptedPublicationWritesDiagnostic()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var eventBus = new InMemoryEventBus(diagnostics: diagnostics);

        var result = await eventBus.PostAsync(new TestEvent("accepted"));

        Assert.True(result.Accepted);
        Assert.Contains(diagnostics.Records, record => record.Code == EventDiagnosticIds.EventAccepted);
    }

    [Fact]
    public async Task PostAsyncRejectedPublicationWritesDiagnostic()
    {
        var diagnostics = new InMemoryHostDiagnostics();
        var eventBus = new InMemoryEventBus(diagnostics: diagnostics);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await eventBus.PostAsync(
            new TestEvent("rejected"),
            cancellationToken: cancellation.Token);

        Assert.False(result.Accepted);
        Assert.Contains(diagnostics.Records, record => record.Code == EventDiagnosticIds.EventRejected);
    }

    private sealed record TestEvent(string Value);
}
