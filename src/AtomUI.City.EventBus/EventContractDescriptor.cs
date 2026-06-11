using System.Reflection;

namespace AtomUI.City.EventBus;

public sealed class EventContractDescriptor
{
    private EventContractDescriptor(
        EventContractId contractId,
        Type eventType,
        EventContractPlane plane,
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(assembly);

        ContractId = contractId;
        EventType = eventType;
        Plane = plane;
        Assembly = assembly;
    }

    public EventContractId ContractId { get; }

    public Type EventType { get; }

    public EventContractPlane Plane { get; }

    public Assembly Assembly { get; }

    public static EventContractDescriptor Shared<TEvent>(
        EventContractId contractId,
        Assembly sharedAssembly)
    {
        ArgumentNullException.ThrowIfNull(sharedAssembly);

        var eventType = typeof(TEvent);
        if (!ReferenceEquals(eventType.Assembly, sharedAssembly))
        {
            throw new InvalidOperationException(
                $"Shared event contract '{eventType.FullName}' must be defined by shared assembly '{sharedAssembly.GetName().Name}'.");
        }

        return new EventContractDescriptor(
            contractId,
            eventType,
            EventContractPlane.Shared,
            sharedAssembly);
    }

    public static EventContractDescriptor PluginPrivate<TEvent>(EventContractId contractId)
    {
        var eventType = typeof(TEvent);

        return new EventContractDescriptor(
            contractId,
            eventType,
            EventContractPlane.PluginPrivate,
            eventType.Assembly);
    }

    public static EventContractDescriptor DefaultShared<TEvent>()
    {
        var eventType = typeof(TEvent);
        var contractName = eventType.FullName ?? eventType.Name;

        return Shared<TEvent>(
            new EventContractId(contractName),
            eventType.Assembly);
    }
}
