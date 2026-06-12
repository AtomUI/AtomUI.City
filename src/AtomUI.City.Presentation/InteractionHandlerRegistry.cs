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
        return Register(
            handler,
            new InteractionHandlerRegistrationOptions
            {
                ActivationScope = activationScope,
            });
    }

    public IDisposable Register<TRequest, TResult>(
        Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
        InteractionHandlerRegistrationOptions options)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(options);

        var key = InteractionHandlerKey.Create<TRequest, TResult>();
        var registration = new HandlerRegistration<TRequest, TResult>(
            this,
            key,
            handler,
            options);

        lock (_gate)
        {
            if (!_registrations.TryGetValue(key, out var registrations))
            {
                registrations = [];
                _registrations[key] = registrations;
            }

            registrations.Add(registration);
        }

        options.ActivationScope?.Add(registration);

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
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, registration.CancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                registration.CancellationToken,
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

    public int RevokePlugin(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        return Revoke(registration => string.Equals(registration.PluginId, pluginId, StringComparison.Ordinal));
    }

    public int RevokeContribution(string contributionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contributionId);

        return Revoke(
            registration => string.Equals(registration.ContributionId, contributionId, StringComparison.Ordinal));
    }

    private HandlerRegistration<TRequest, TResult>? FindLastRegistration<TRequest, TResult>()
    {
        var key = InteractionHandlerKey.Create<TRequest, TResult>();

        lock (_gate)
        {
            return _registrations.TryGetValue(key, out var registrations)
                ? registrations
                    .OfType<HandlerRegistration<TRequest, TResult>>()
                    .LastOrDefault(registration => !registration.IsDisposed)
                : null;
        }
    }

    private int Revoke(Func<HandlerRegistration, bool> predicate)
    {
        List<HandlerRegistration> registrations;

        lock (_gate)
        {
            registrations = _registrations
                .Values
                .SelectMany(static items => items)
                .Where(registration => !registration.IsDisposed && predicate(registration))
                .ToList();
        }

        foreach (var registration in registrations)
        {
            registration.Dispose();
            WriteRevokedDiagnostic(registration);
        }

        return registrations.Count;
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

    private void WriteRevokedDiagnostic(HandlerRegistration registration)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.InteractionHandlerRevoked,
            $"Presentation interaction handler revoked plugin '{Normalize(registration.PluginId)}' contribution '{Normalize(registration.ContributionId)}'.",
            HostDiagnosticSeverity.Info));
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "<none>" : value;
    }

    private abstract class HandlerRegistration : IDisposable
    {
        private readonly CancellationTokenSource _cancellation = new();
        private readonly InteractionHandlerRegistry _registry;

        protected HandlerRegistration(
            InteractionHandlerRegistry registry,
            InteractionHandlerKey key,
            InteractionHandlerRegistrationOptions options)
        {
            _registry = registry;
            Key = key;
            ActivationScope = options.ActivationScope;
            PluginId = string.IsNullOrWhiteSpace(options.PluginId) ? null : options.PluginId;
            ContributionId = string.IsNullOrWhiteSpace(options.ContributionId) ? null : options.ContributionId;
        }

        public InteractionHandlerKey Key { get; }

        public IActivationScope? ActivationScope { get; }

        public CancellationToken CancellationToken => _cancellation.Token;

        public string? PluginId { get; }

        public string? ContributionId { get; }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            _cancellation.Cancel();
            _registry.Remove(this);
            _cancellation.Dispose();
        }
    }

    private sealed class HandlerRegistration<TRequest, TResult> : HandlerRegistration
    {
        private readonly Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> _handler;

        public HandlerRegistration(
            InteractionHandlerRegistry registry,
            InteractionHandlerKey key,
            Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
            InteractionHandlerRegistrationOptions options)
            : base(registry, key, options)
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
