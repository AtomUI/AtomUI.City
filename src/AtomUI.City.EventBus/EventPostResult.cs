namespace AtomUI.City.EventBus;

public sealed record EventPostResult(
    Guid EventId,
    EventContractId ContractId,
    bool Accepted,
    string? RejectionReason = null);
