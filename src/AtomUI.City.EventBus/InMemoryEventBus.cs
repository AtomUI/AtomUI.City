using AtomUI.City.Diagnostics;
using AtomUI.City.Lifecycle;

namespace AtomUI.City.EventBus;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly IEventContractRegistry _contractRegistry;
    private readonly IHostDiagnostics? _diagnostics;
    private readonly Dictionary<Type, List<EventSubscription>> _subscriptions = [];
    private readonly object _syncRoot = new();

    public InMemoryEventBus(
        IEventContractRegistry? contractRegistry = null,
        IHostDiagnostics? diagnostics = null)
    {
        _contractRegistry = contractRegistry ?? new InMemoryEventContractRegistry();
        _diagnostics = diagnostics;
    }

    public IEventSubscription Subscribe<TEvent>(
        Func<EventContext<TEvent>, ValueTask> handler,
        EventSubscriptionOptions? options = null)
    {
        return SubscribeCore(
            owner: null,
            handler,
            options ?? EventSubscriptionOptions.Serialized);
    }

    public IEventSubscription Subscribe<TEvent>(
        Action<EventContext<TEvent>> handler,
        EventSubscriptionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return Subscribe<TEvent>(
            context =>
            {
                handler(context);

                return ValueTask.CompletedTask;
            },
            options);
    }

    public IEventSubscription Subscribe<TEvent>(
        IEventHandler<TEvent> handler,
        EventSubscriptionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return Subscribe<TEvent>(
            context => handler.HandleAsync(context),
            options);
    }

    public IEventSubscription Subscribe<TEvent>(
        LifecycleScope owner,
        Func<EventContext<TEvent>, ValueTask> handler,
        EventSubscriptionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return SubscribeCore(
            owner,
            handler,
            options ?? EventSubscriptionOptions.Serialized);
    }

    public IEventSubscription Subscribe<TEvent>(
        LifecycleScope owner,
        Action<EventContext<TEvent>> handler,
        EventSubscriptionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return Subscribe<TEvent>(
            owner,
            context =>
            {
                handler(context);

                return ValueTask.CompletedTask;
            },
            options);
    }

    public IEventSubscription Subscribe<TEvent>(
        LifecycleScope owner,
        IEventHandler<TEvent> handler,
        EventSubscriptionOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return Subscribe<TEvent>(
            owner,
            context => handler.HandleAsync(context),
            options);
    }

    public IEventSubscription Subscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        return Subscribe<TEvent>(
            context => handler(context.Event, context.CancellationToken),
            EventSubscriptionOptions.Serialized);
    }

    public async ValueTask<EventPublishResult> PublishAsync<TEvent>(
        TEvent eventData,
        EventPublishOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await PublishCoreAsync(
                eventData,
                options,
                Guid.NewGuid(),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<EventPublishResult> PublishCoreAsync<TEvent>(
        TEvent eventData,
        EventPublishOptions? options,
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var descriptor = _contractRegistry.GetOrCreate<TEvent>();
        var publishOptions = options ?? EventPublishOptions.Default;
        var publishedAt = DateTimeOffset.UtcNow;
        var correlationId = string.IsNullOrWhiteSpace(publishOptions.CorrelationId)
            ? eventId.ToString("D")
            : publishOptions.CorrelationId;
        var snapshot = GetSnapshot(typeof(TEvent));
        var deliveries = new List<EventDeliveryResult>(snapshot.Length);

        WriteDiagnostic(
            EventDiagnosticIds.EventPublished,
            $"Event '{descriptor.ContractId.Value}' was published.",
            HostDiagnosticSeverity.Trace);

        foreach (var subscription in snapshot)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (deliveries.Count > 0)
                {
                    break;
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            var delivery = await subscription.DeliverAsync(
                    eventData!,
                    descriptor,
                    eventId,
                    correlationId,
                    publishOptions.CausationId,
                    publishedAt,
                    publishOptions.PublishDepth,
                    cancellationToken)
                .ConfigureAwait(false);

            if (delivery is null)
            {
                continue;
            }

            deliveries.Add(delivery);

            if (delivery.Canceled)
            {
                break;
            }

            if (!delivery.Succeeded
                && subscription.Options.ErrorPolicy == EventErrorPolicy.StopPublication)
            {
                break;
            }
        }

        return new EventPublishResult(
            eventId,
            descriptor.ContractId,
            deliveries);
    }

    public ValueTask<EventPostResult> PostAsync<TEvent>(
        TEvent eventData,
        EventPublishOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var descriptor = _contractRegistry.GetOrCreate<TEvent>();
        var eventId = Guid.NewGuid();

        if (cancellationToken.IsCancellationRequested)
        {
            WriteDiagnostic(
                EventDiagnosticIds.EventRejected,
                $"Posted event '{descriptor.ContractId.Value}' was rejected because publication was canceled before acceptance.",
                HostDiagnosticSeverity.Warning);

            return ValueTask.FromResult(
                new EventPostResult(
                    eventId,
                    descriptor.ContractId,
                    Accepted: false,
                    "Publication was canceled before it was accepted."));
        }

        WriteDiagnostic(
            EventDiagnosticIds.EventAccepted,
            $"Posted event '{descriptor.ContractId.Value}' was accepted.",
            HostDiagnosticSeverity.Trace);

        _ = Task.Run(
            async () =>
            {
                try
                {
                    await PublishCoreAsync(
                            eventData,
                            options,
                            eventId,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    WriteDiagnostic(
                        EventDiagnosticIds.EventDeliveryFailed,
                        $"Posted event '{descriptor.ContractId.Value}' failed: {exception.Message}",
                        HostDiagnosticSeverity.Error);
                }
            },
            CancellationToken.None);

        return ValueTask.FromResult(
            new EventPostResult(
                eventId,
                descriptor.ContractId,
                Accepted: true));
    }

    private IEventSubscription SubscribeCore<TEvent>(
        LifecycleScope? owner,
        Func<EventContext<TEvent>, ValueTask> handler,
        EventSubscriptionOptions options)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(options);

        var subscription = new EventSubscription(
            this,
            typeof(TEvent),
            options,
            (eventData, delivery) =>
            {
                var context = new EventContext<TEvent>(
                    (TEvent)eventData,
                    delivery.ContractId,
                    delivery.EventId,
                    delivery.CorrelationId,
                    delivery.CausationId,
                    delivery.PublishedAt,
                    delivery.PublishDepth,
                    delivery.SubscriptionId,
                    delivery.DispatchPolicy,
                    delivery.CancellationToken);

                return handler(context);
            },
            _diagnostics,
            owner);

        lock (_syncRoot)
        {
            if (!_subscriptions.TryGetValue(subscription.EventType, out var subscriptions))
            {
                subscriptions = [];
                _subscriptions[subscription.EventType] = subscriptions;
            }

            subscriptions.Add(subscription);
            subscription.MarkActive();
        }

        WriteDiagnostic(
            EventDiagnosticIds.EventSubscriptionAdded,
            $"Event subscription '{subscription.Id}' was added.",
            HostDiagnosticSeverity.Trace);

        return subscription;
    }

    private EventSubscription[] GetSnapshot(Type eventType)
    {
        lock (_syncRoot)
        {
            return _subscriptions.TryGetValue(eventType, out var subscriptions)
                ? subscriptions
                    .Where(subscription => subscription.State == EventSubscriptionState.Active)
                    .ToArray()
                : [];
        }
    }

    private void Remove(EventSubscription subscription)
    {
        lock (_syncRoot)
        {
            if (!_subscriptions.TryGetValue(subscription.EventType, out var subscriptions))
            {
                return;
            }

            subscriptions.Remove(subscription);
            if (subscriptions.Count == 0)
            {
                _subscriptions.Remove(subscription.EventType);
            }
        }

        WriteDiagnostic(
            EventDiagnosticIds.EventSubscriptionDisposed,
            $"Event subscription '{subscription.Id}' was disposed.",
            HostDiagnosticSeverity.Trace);
    }

    private void WriteDiagnostic(
        string code,
        string message,
        HostDiagnosticSeverity severity)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(code, message, severity));
    }

    private readonly record struct DeliveryContext(
        EventContractId ContractId,
        Guid EventId,
        string CorrelationId,
        string? CausationId,
        DateTimeOffset PublishedAt,
        int PublishDepth,
        EventSubscriptionId SubscriptionId,
        EventDispatchPolicy DispatchPolicy,
        CancellationToken CancellationToken);

    private sealed class EventSubscription : IEventSubscription
    {
        private readonly InMemoryEventBus _eventBus;
        private readonly Func<object, DeliveryContext, ValueTask> _handler;
        private readonly IHostDiagnostics? _diagnostics;
        private readonly SemaphoreSlim _serialGate = new(1, 1);
        private readonly object _stateGate = new();
        private readonly CancellationTokenRegistration _ownerCancellation;
        private TaskCompletionSource? _drainCompletion;
        private int _inFlightCount;
        private EventSubscriptionState _state = EventSubscriptionState.Created;

        public EventSubscription(
            InMemoryEventBus eventBus,
            Type eventType,
            EventSubscriptionOptions options,
            Func<object, DeliveryContext, ValueTask> handler,
            IHostDiagnostics? diagnostics,
            LifecycleScope? owner)
        {
            _eventBus = eventBus;
            EventType = eventType;
            Options = options;
            _handler = handler;
            _diagnostics = diagnostics;
            Id = EventSubscriptionId.New();

            if (owner is not null)
            {
                _ownerCancellation = owner.CancellationToken.Register(
                    static state => ((EventSubscription)state!).Dispose(),
                    this);
            }
        }

        public EventSubscriptionId Id { get; }

        public Type EventType { get; }

        public EventSubscriptionOptions Options { get; }

        public EventSubscriptionState State
        {
            get
            {
                lock (_stateGate)
                {
                    return _state;
                }
            }
        }

        public void MarkActive()
        {
            lock (_stateGate)
            {
                if (_state == EventSubscriptionState.Created)
                {
                    _state = EventSubscriptionState.Active;
                }
            }
        }

        public async ValueTask<EventDeliveryResult?> DeliverAsync(
            object eventData,
            EventContractDescriptor descriptor,
            Guid eventId,
            string correlationId,
            string? causationId,
            DateTimeOffset publishedAt,
            int publishDepth,
            CancellationToken cancellationToken)
        {
            if (State != EventSubscriptionState.Active)
            {
                return null;
            }

            if (Options.DispatchPolicy == EventDispatchPolicy.Serialized)
            {
                await _serialGate.WaitAsync(cancellationToken).ConfigureAwait(false);
                var acquiredDelivery = false;

                try
                {
                    acquiredDelivery = TryBeginDelivery();
                    if (!acquiredDelivery)
                    {
                        return null;
                    }

                    return await DispatchAsync(
                            eventData,
                            descriptor,
                            eventId,
                            correlationId,
                            causationId,
                            publishedAt,
                            publishDepth,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                finally
                {
                    if (acquiredDelivery)
                    {
                        EndDelivery();
                    }

                    _serialGate.Release();
                }
            }

            if (!TryBeginDelivery())
            {
                return null;
            }

            try
            {
                return await DispatchAsync(
                        eventData,
                        descriptor,
                        eventId,
                        correlationId,
                        causationId,
                        publishedAt,
                        publishDepth,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                EndDelivery();
            }
        }

        public async ValueTask StopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Task? drainTask;
            var removeFromBus = false;

            lock (_stateGate)
            {
                if (_state == EventSubscriptionState.Disposed)
                {
                    return;
                }

                if (_state != EventSubscriptionState.Quiescing)
                {
                    _state = EventSubscriptionState.Quiescing;
                    removeFromBus = true;
                }

                if (_inFlightCount > 0)
                {
                    _drainCompletion ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                    drainTask = _drainCompletion.Task;
                }
                else
                {
                    drainTask = null;
                }
            }

            if (removeFromBus)
            {
                _eventBus.Remove(this);
                _ownerCancellation.Dispose();
            }

            if (drainTask is not null)
            {
                await drainTask.WaitAsync(cancellationToken).ConfigureAwait(false);
            }

            lock (_stateGate)
            {
                _state = EventSubscriptionState.Disposed;
            }
        }

        public void Dispose()
        {
            StopAsync().AsTask().GetAwaiter().GetResult();
        }

        public ValueTask DisposeAsync()
        {
            return StopAsync();
        }

        private async ValueTask<EventDeliveryResult> DispatchAsync(
            object eventData,
            EventContractDescriptor descriptor,
            Guid eventId,
            string correlationId,
            string? causationId,
            DateTimeOffset publishedAt,
            int publishDepth,
            CancellationToken cancellationToken)
        {
            try
            {
                var delivery = new DeliveryContext(
                    descriptor.ContractId,
                    eventId,
                    correlationId,
                    causationId,
                    publishedAt,
                    publishDepth,
                    Id,
                    Options.DispatchPolicy,
                    cancellationToken);

                await DispatchCoreAsync(eventData, delivery, cancellationToken).ConfigureAwait(false);

                return new EventDeliveryResult(
                    Id,
                    Options.DispatchPolicy,
                    Succeeded: true);
            }
            catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
            {
                _diagnostics?.Write(
                    new HostDiagnosticRecord(
                        EventDiagnosticIds.EventDeliveryCancelled,
                        $"Event handler '{Id}' was cancelled: {exception.Message}",
                        HostDiagnosticSeverity.Trace));

                return new EventDeliveryResult(
                    Id,
                    Options.DispatchPolicy,
                    Succeeded: false,
                    exception.Message,
                    Canceled: true);
            }
            catch (Exception exception)
            {
                _diagnostics?.Write(
                    new HostDiagnosticRecord(
                        EventDiagnosticIds.EventDeliveryFailed,
                        $"Event handler '{Id}' failed: {exception.Message}",
                        HostDiagnosticSeverity.Error));

                if (Options.ErrorPolicy == EventErrorPolicy.FailPublisher)
                {
                    throw;
                }

                return new EventDeliveryResult(
                    Id,
                    Options.DispatchPolicy,
                    Succeeded: false,
                    exception.Message);
            }
        }

        private async ValueTask DispatchCoreAsync(
            object eventData,
            DeliveryContext delivery,
            CancellationToken cancellationToken)
        {
            switch (Options.DispatchPolicy)
            {
                case EventDispatchPolicy.UiThread:
                    if (Options.UiDispatcher is null)
                    {
                        throw new InvalidOperationException("UI dispatcher subscription requires a dispatcher.");
                    }

                    await Options.UiDispatcher.PostAsync(
                            async token => await _handler(eventData, delivery with { CancellationToken = token }).ConfigureAwait(false),
                            cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case EventDispatchPolicy.Background:
                    await Task.Run(
                            async () => await _handler(eventData, delivery).ConfigureAwait(false),
                            cancellationToken)
                        .ConfigureAwait(false);
                    break;
                default:
                    await _handler(eventData, delivery).ConfigureAwait(false);
                    break;
            }
        }

        private bool TryBeginDelivery()
        {
            lock (_stateGate)
            {
                if (_state != EventSubscriptionState.Active)
                {
                    return false;
                }

                _inFlightCount++;

                return true;
            }
        }

        private void EndDelivery()
        {
            TaskCompletionSource? drainCompletion = null;

            lock (_stateGate)
            {
                _inFlightCount--;

                if (_inFlightCount == 0 && _state == EventSubscriptionState.Quiescing)
                {
                    drainCompletion = _drainCompletion;
                }
            }

            drainCompletion?.TrySetResult();
        }
    }
}
