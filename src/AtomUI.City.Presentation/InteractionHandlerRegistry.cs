using AtomUI.City.Diagnostics;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class InteractionHandlerRegistry : IInteractionHandlerRegistry
{
    private readonly object _gate = new();
    private readonly Dictionary<InteractionHandlerKey, List<HandlerRegistration>> _registrations = [];
    private readonly IUiDispatcher _dispatcher;
    private readonly IHostDiagnostics? _diagnostics;

    public InteractionHandlerRegistry(IUiDispatcher dispatcher)
        : this(dispatcher, diagnostics: null)
    {
    }

    public InteractionHandlerRegistry(
        IUiDispatcher dispatcher,
        IHostDiagnostics? diagnostics)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        _dispatcher = dispatcher;
        _diagnostics = diagnostics;
    }

    public IDisposable Register<TRequest, TResult>(
        Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
        IActivationScope? activationScope = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var key = InteractionHandlerKey.Create<TRequest, TResult>();
        var registration = new HandlerRegistration<TRequest, TResult>(
            this,
            key,
            handler,
            activationScope);

        lock (_gate)
        {
            if (!_registrations.TryGetValue(key, out var registrations))
            {
                registrations = [];
                _registrations[key] = registrations;
            }

            registrations.Add(registration);
        }

        activationScope?.Add(registration);

        return registration;
    }

    public async ValueTask<InteractionResult<TResult>> HandleAsync<TRequest, TResult>(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var registration = FindLastRegistration<TRequest, TResult>();
        if (registration is null)
        {
            WriteNotHandledDiagnostic<TRequest, TResult>();

            return InteractionResult<TResult>.NotHandled();
        }

        using var linkedCancellation = registration.ActivationScope is null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                registration.ActivationScope.CancellationToken);

        try
        {
            TResult? value = default;
            await _dispatcher.PostAsync(
                async dispatcherCancellationToken =>
                {
                    using var executionCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                        linkedCancellation.Token,
                        dispatcherCancellationToken);
                    var context = new InteractionContext<TRequest>(request);
                    value = await registration
                        .HandleAsync(context, executionCancellation.Token)
                        .ConfigureAwait(false);
                },
                linkedCancellation.Token).ConfigureAwait(false);

            WriteHandledDiagnostic<TRequest, TResult>();

            return InteractionResult<TResult>.Completed(value!);
        }
        catch (OperationCanceledException)
            when (linkedCancellation.IsCancellationRequested)
        {
            return InteractionResult<TResult>.Canceled();
        }
        catch (Exception exception)
        {
            WriteFailedDiagnostic<TRequest, TResult>(exception);

            return InteractionResult<TResult>.Failed(exception);
        }
    }

    private HandlerRegistration<TRequest, TResult>? FindLastRegistration<TRequest, TResult>()
    {
        var key = InteractionHandlerKey.Create<TRequest, TResult>();

        lock (_gate)
        {
            return _registrations.TryGetValue(key, out var registrations)
                ? registrations.OfType<HandlerRegistration<TRequest, TResult>>().LastOrDefault()
                : null;
        }
    }

    private void Remove(HandlerRegistration registration)
    {
        lock (_gate)
        {
            if (!_registrations.TryGetValue(registration.Key, out var registrations))
            {
                return;
            }

            registrations.Remove(registration);
            if (registrations.Count == 0)
            {
                _registrations.Remove(registration.Key);
            }
        }
    }

    private void WriteHandledDiagnostic<TRequest, TResult>()
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.InteractionHandled,
            $"Presentation interaction handler completed request '{typeof(TRequest).FullName}' with result '{typeof(TResult).FullName}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteNotHandledDiagnostic<TRequest, TResult>()
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.InteractionNotHandled,
            $"Presentation interaction handler was not found for request '{typeof(TRequest).FullName}' with result '{typeof(TResult).FullName}'.",
            HostDiagnosticSeverity.Warning));
    }

    private void WriteFailedDiagnostic<TRequest, TResult>(Exception exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.InteractionFailed,
            $"Presentation interaction handler failed for request '{typeof(TRequest).FullName}' with result '{typeof(TResult).FullName}': {exception.Message}",
            HostDiagnosticSeverity.Error));
    }

    private abstract class HandlerRegistration : IDisposable
    {
        private readonly InteractionHandlerRegistry _registry;
        private bool _disposed;

        protected HandlerRegistration(
            InteractionHandlerRegistry registry,
            InteractionHandlerKey key,
            IActivationScope? activationScope)
        {
            _registry = registry;
            Key = key;
            ActivationScope = activationScope;
        }

        public InteractionHandlerKey Key { get; }

        public IActivationScope? ActivationScope { get; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _registry.Remove(this);
        }
    }

    private sealed class HandlerRegistration<TRequest, TResult> : HandlerRegistration
    {
        private readonly Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> _handler;

        public HandlerRegistration(
            InteractionHandlerRegistry registry,
            InteractionHandlerKey key,
            Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
            IActivationScope? activationScope)
            : base(registry, key, activationScope)
        {
            _handler = handler;
        }

        public ValueTask<TResult> HandleAsync(
            InteractionContext<TRequest> context,
            CancellationToken cancellationToken)
        {
            return _handler(context, cancellationToken);
        }
    }

    private readonly record struct InteractionHandlerKey(
        Type RequestType,
        Type ResultType)
    {
        public static InteractionHandlerKey Create<TRequest, TResult>()
        {
            return new InteractionHandlerKey(typeof(TRequest), typeof(TResult));
        }
    }
}
