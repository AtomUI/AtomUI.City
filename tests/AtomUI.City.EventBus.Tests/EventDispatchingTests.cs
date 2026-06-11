using AtomUI.City.EventBus;
using AtomUI.City.Threading;

namespace AtomUI.City.EventBus.Tests;

public sealed class EventDispatchingTests
{
    [Fact]
    public async Task UiThreadSubscriptionUsesUiDispatcher()
    {
        var dispatcher = new RecordingDispatcher();
        var eventBus = new InMemoryEventBus();
        var received = string.Empty;

        eventBus.Subscribe<TestEvent>(
            context =>
            {
                received = context.Event.Value;
                return ValueTask.CompletedTask;
            },
            EventSubscriptionOptions.UiThread(dispatcher));

        var result = await eventBus.PublishAsync(new TestEvent("ui"));

        Assert.True(result.Succeeded);
        Assert.Equal("ui", received);
        Assert.Equal(1, dispatcher.PostCount);
        Assert.Equal(EventDispatchPolicy.UiThread, Assert.Single(result.Deliveries).DispatchPolicy);
    }

    [Fact]
    public async Task BackgroundSubscriptionRecordsBackgroundDispatchPolicy()
    {
        var eventBus = new InMemoryEventBus();
        var received = string.Empty;

        eventBus.Subscribe<TestEvent>(
            context =>
            {
                received = context.Event.Value;
                return ValueTask.CompletedTask;
            },
            EventSubscriptionOptions.Background());

        var result = await eventBus.PublishAsync(new TestEvent("background"));

        Assert.True(result.Succeeded);
        Assert.Equal("background", received);
        Assert.Equal(EventDispatchPolicy.Background, Assert.Single(result.Deliveries).DispatchPolicy);
    }

    private sealed record TestEvent(string Value);

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public int PostCount { get; private set; }

        public bool CheckAccess() => true;

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(callback());
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            PostCount++;

            return callback(cancellationToken);
        }
    }
}
