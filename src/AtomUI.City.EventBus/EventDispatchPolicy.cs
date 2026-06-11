namespace AtomUI.City.EventBus;

public enum EventDispatchPolicy
{
    Current,
    UiThread,
    Background,
    Serialized,
}
