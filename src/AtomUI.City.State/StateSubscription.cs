using AtomUI.City.Threading;

namespace AtomUI.City.State;

internal sealed class StateSubscription : IStateSubscription
{
    private readonly Action<StateChangedEventArgs> _handler;
    private readonly StateSubscriptionOptions _options;
    private bool _disposed;

    public StateSubscription(
        Action<StateChangedEventArgs> handler,
        StateSubscriptionOptions options)
    {
        _handler = handler;
        _options = options;
    }

    public void Notify(StateChangedEventArgs args)
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            switch (_options.DispatchPolicy)
            {
                case StateDispatchPolicy.Dispatcher:
                    _options.UiDispatcher?.InvokeAsync(() => _handler(args)).AsTask().GetAwaiter().GetResult();
                    break;
                case StateDispatchPolicy.Background:
                    Task.Run(() => _handler(args)).GetAwaiter().GetResult();
                    break;
                default:
                    _handler(args);
                    break;
            }
        }
        catch
        {
            // Diagnostics integration is handled by the next State diagnostics phase.
        }
    }

    public void Dispose()
    {
        _disposed = true;
    }
}

public sealed class StateSubscriptionOptions
{
    private StateSubscriptionOptions(
        StateDispatchPolicy dispatchPolicy,
        IUiDispatcher? dispatcher)
    {
        DispatchPolicy = dispatchPolicy;
        UiDispatcher = dispatcher;
    }

    public static StateSubscriptionOptions Immediate { get; } = new(
        StateDispatchPolicy.Immediate,
        dispatcher: null);

    public StateDispatchPolicy DispatchPolicy { get; }

    public IUiDispatcher? UiDispatcher { get; }

    public static StateSubscriptionOptions Dispatcher(IUiDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        return new StateSubscriptionOptions(
            StateDispatchPolicy.Dispatcher,
            dispatcher);
    }
}
