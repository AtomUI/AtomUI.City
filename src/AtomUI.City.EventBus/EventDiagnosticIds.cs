namespace AtomUI.City.EventBus;

public static class EventDiagnosticIds
{
    public const string EventPublished = "EventBus.EventPublished";
    public const string EventAccepted = "EventBus.EventAccepted";
    public const string EventRejected = "EventBus.EventRejected";
    public const string EventDeliveryFailed = "EventBus.EventDeliveryFailed";
    public const string EventSubscriptionAdded = "EventBus.EventSubscriptionAdded";
    public const string EventSubscriptionDisposed = "EventBus.EventSubscriptionDisposed";
}
