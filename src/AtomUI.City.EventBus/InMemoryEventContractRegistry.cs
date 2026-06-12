namespace AtomUI.City.EventBus;

public sealed class InMemoryEventContractRegistry : IEventContractRegistry
{
    private readonly Dictionary<EventContractId, EventContractDescriptor> _byContractId = [];
    private readonly Dictionary<Type, EventContractDescriptor> _byEventType = [];
    private readonly object _syncRoot = new();

    public void Register(EventContractDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        lock (_syncRoot)
        {
            RegisterCore(descriptor);
        }
    }

    public EventContractDescriptor GetOrCreate<TEvent>()
    {
        var eventType = typeof(TEvent);

        lock (_syncRoot)
        {
            if (_byEventType.TryGetValue(eventType, out var descriptor))
            {
                return descriptor;
            }

            descriptor = EventContractDescriptor.DefaultShared<TEvent>();
            RegisterCore(descriptor);

            return descriptor;
        }
    }

    private void RegisterCore(EventContractDescriptor descriptor)
    {
        if (_byContractId.TryGetValue(descriptor.ContractId, out var existingContract)
            && existingContract.EventType != descriptor.EventType)
        {
            throw new InvalidOperationException(
                $"Event contract id '{descriptor.ContractId.Value}' is already registered for '{existingContract.EventType.FullName}'.");
        }

        if (_byEventType.TryGetValue(descriptor.EventType, out var existingType)
            && existingType.ContractId != descriptor.ContractId)
        {
            throw new InvalidOperationException(
                $"Event type '{descriptor.EventType.FullName}' is already registered as '{existingType.ContractId.Value}'.");
        }

        _byContractId[descriptor.ContractId] = descriptor;
        _byEventType[descriptor.EventType] = descriptor;
    }
}
