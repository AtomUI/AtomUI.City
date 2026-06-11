namespace AtomUI.City.EventBus;

public interface IEventContractRegistry
{
    void Register(EventContractDescriptor descriptor);

    EventContractDescriptor GetOrCreate<TEvent>();
}
