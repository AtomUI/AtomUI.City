namespace AtomUI.City.EventBus;

public enum EventSubscriptionState
{
    Created,
    Active,
    Quiescing,
    Disposed,
    Faulted,
}
