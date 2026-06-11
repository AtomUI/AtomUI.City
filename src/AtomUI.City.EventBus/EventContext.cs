namespace AtomUI.City.EventBus;

public sealed class EventContext<TEvent>
{
    public EventContext(
        TEvent eventData,
        EventContractId contractId,
        Guid eventId,
        string correlationId,
        string? causationId,
        DateTimeOffset publishedAt,
        int publishDepth,
        EventSubscriptionId subscriptionId,
        EventDispatchPolicy dispatchPolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        Event = eventData;
        ContractId = contractId;
        EventId = eventId;
        CorrelationId = correlationId;
        CausationId = causationId;
        PublishedAt = publishedAt;
        PublishDepth = publishDepth;
        SubscriptionId = subscriptionId;
        DispatchPolicy = dispatchPolicy;
        CancellationToken = cancellationToken;
    }

    public TEvent Event { get; }

    public EventContractId ContractId { get; }

    public Guid EventId { get; }

    public string CorrelationId { get; }

    public string? CausationId { get; }

    public DateTimeOffset PublishedAt { get; }

    public int PublishDepth { get; }

    public EventSubscriptionId SubscriptionId { get; }

    public EventDispatchPolicy DispatchPolicy { get; }

    public CancellationToken CancellationToken { get; }
}
