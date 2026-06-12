using AtomUI.City.Threading;

namespace AtomUI.City.EventBus;

public sealed class EventSubscriptionOptions
{
    private EventSubscriptionOptions(
        EventDispatchPolicy dispatchPolicy,
        IUiDispatcher? uiDispatcher,
        EventErrorPolicy errorPolicy)
    {
        DispatchPolicy = dispatchPolicy;
        UiDispatcher = uiDispatcher;
        ErrorPolicy = errorPolicy;
    }

    public static EventSubscriptionOptions Serialized { get; } = new(
        EventDispatchPolicy.Serialized,
        uiDispatcher: null,
        EventErrorPolicy.ContinueAndReport);

    public static EventSubscriptionOptions Current { get; } = new(
        EventDispatchPolicy.Current,
        uiDispatcher: null,
        EventErrorPolicy.ContinueAndReport);

    public EventDispatchPolicy DispatchPolicy { get; }

    public IUiDispatcher? UiDispatcher { get; }

    public EventErrorPolicy ErrorPolicy { get; }

    public static EventSubscriptionOptions UiThread(IUiDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        return new EventSubscriptionOptions(
            EventDispatchPolicy.UiThread,
            dispatcher,
            EventErrorPolicy.ContinueAndReport);
    }

    public static EventSubscriptionOptions Background()
    {
        return new EventSubscriptionOptions(
            EventDispatchPolicy.Background,
            uiDispatcher: null,
            EventErrorPolicy.ContinueAndReport);
    }

    public EventSubscriptionOptions WithErrorPolicy(EventErrorPolicy errorPolicy)
    {
        return new EventSubscriptionOptions(
            DispatchPolicy,
            UiDispatcher,
            errorPolicy);
    }
}
