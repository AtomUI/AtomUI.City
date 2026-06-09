namespace AtomUI.City.EventBus;

public interface IEventBus
{
    IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler);

    ValueTask PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default);
}
