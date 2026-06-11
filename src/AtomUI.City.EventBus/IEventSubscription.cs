namespace AtomUI.City.EventBus;

public interface IEventSubscription : IDisposable, IAsyncDisposable
{
    EventSubscriptionId Id { get; }

    EventSubscriptionState State { get; }

    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
