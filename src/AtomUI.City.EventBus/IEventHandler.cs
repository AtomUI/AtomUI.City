namespace AtomUI.City.EventBus;

public interface IEventHandler<TEvent>
{
    ValueTask HandleAsync(EventContext<TEvent> context);
}
