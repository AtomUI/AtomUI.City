using AtomUI.City.Diagnostics;
using AtomUI.City.Threading;

namespace AtomUI.City.State;

internal sealed class StateSubscription : IStateSubscription
{
    private readonly Action<StateChangedEventArgs> _handler;
    private readonly IHostDiagnostics? _diagnostics;
    private readonly StateSubscriptionOptions _options;
    private readonly Queue<StateChangedEventArgs> _queuedNotifications = [];
    private readonly object _queueSyncRoot = new();
    private bool _disposed;
    private bool _isProcessingQueue;

    public StateSubscription(
        Action<StateChangedEventArgs> handler,
        StateSubscriptionOptions options,
        IHostDiagnostics? diagnostics = null)
    {
        _handler = handler;
        _options = options;
        _diagnostics = diagnostics;
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
                case StateDispatchPolicy.Queued:
                    Enqueue(args);
                    break;
                default:
                    _handler(args);
                    break;
            }
        }
        catch (Exception exception)
        {
            _diagnostics?.Write(new HostDiagnosticRecord(
                StateDiagnosticIds.SubscriptionHandlerFailed,
                $"State subscription handler failed at version {args.Version}: {exception.Message}",
                HostDiagnosticSeverity.Error));
        }
    }

    public void Dispose()
    {
        _disposed = true;

        lock (_queueSyncRoot)
        {
            _queuedNotifications.Clear();
            _isProcessingQueue = false;
        }
    }

    private void Enqueue(StateChangedEventArgs args)
    {
        var shouldStartProcessing = false;

        lock (_queueSyncRoot)
        {
            if (_disposed)
            {
                return;
            }

            _queuedNotifications.Enqueue(args);

            if (!_isProcessingQueue)
            {
                _isProcessingQueue = true;
                shouldStartProcessing = true;
            }
        }

        if (shouldStartProcessing)
        {
            _ = Task.Run(ProcessQueue);
        }
    }

    private void ProcessQueue()
    {
        while (true)
        {
            StateChangedEventArgs args;

            lock (_queueSyncRoot)
            {
                if (_disposed || _queuedNotifications.Count == 0)
                {
                    _isProcessingQueue = false;
                    return;
                }

                args = _queuedNotifications.Dequeue();
            }

            NotifyQueued(args);
        }
    }

    private void NotifyQueued(StateChangedEventArgs args)
    {
        try
        {
            _handler(args);
        }
        catch (Exception exception)
        {
            _diagnostics?.Write(new HostDiagnosticRecord(
                StateDiagnosticIds.SubscriptionHandlerFailed,
                $"State subscription handler failed at version {args.Version}: {exception.Message}",
                HostDiagnosticSeverity.Error));
        }
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

    public static StateSubscriptionOptions Background()
    {
        return new StateSubscriptionOptions(
            StateDispatchPolicy.Background,
            dispatcher: null);
    }

    public static StateSubscriptionOptions Queued()
    {
        return new StateSubscriptionOptions(
            StateDispatchPolicy.Queued,
            dispatcher: null);
    }
}
