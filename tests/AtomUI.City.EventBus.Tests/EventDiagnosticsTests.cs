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

    private sealed record TestEvent(string Value);
}
