namespace AtomUI.City.EventBus;

public sealed class EventPublishResult
{
    public EventPublishResult(
        Guid eventId,
        EventContractId contractId,
        IReadOnlyList<EventDeliveryResult> deliveries)
    {
        ArgumentNullException.ThrowIfNull(deliveries);

        EventId = eventId;
        ContractId = contractId;
        Deliveries = Array.AsReadOnly(deliveries.ToArray());
    }

    public Guid EventId { get; }

    public EventContractId ContractId { get; }

    public IReadOnlyList<EventDeliveryResult> Deliveries { get; }

    public int DeliveredCount => Deliveries.Count;

    public int FailedCount => Deliveries.Count(delivery => !delivery.Succeeded && !delivery.Canceled);

    public int CanceledCount => Deliveries.Count(delivery => delivery.Canceled);

    public bool Succeeded => FailedCount == 0 && CanceledCount == 0;
}

public sealed record EventDeliveryResult(
    EventSubscriptionId SubscriptionId,
    EventDispatchPolicy DispatchPolicy,
    bool Succeeded,
    string? ErrorMessage = null,
    bool Canceled = false);
