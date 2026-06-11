namespace AtomUI.City.Mvvm;

public sealed class Interaction<TRequest, TResult>
{
    private readonly object _gate = new();
    private readonly List<HandlerRegistration> _handlers = [];

    public IDisposable RegisterHandler(
        Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
        IActivationScope? activationScope = null)
    {
        ArgumentNullException.ThrowIfNull(handler);

        var registration = new HandlerRegistration(this, handler, activationScope);

        lock (_gate)
        {
            _handlers.Add(registration);
        }

        activationScope?.Add(registration);

        return registration;
    }

    public async ValueTask<InteractionResult<TResult>> RequestAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        HandlerRegistration? registration;

        lock (_gate)
        {
            registration = _handlers.LastOrDefault();
        }

        if (registration is null)
        {
            return InteractionResult<TResult>.NotHandled();
        }

        using var linkedCancellationTokenSource = registration.ActivationScope is null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, registration.ActivationScope.CancellationToken);

        try
        {
            var context = new InteractionContext<TRequest>(request);
            var value = await registration.Handler(context, linkedCancellationTokenSource.Token).ConfigureAwait(false);

            return InteractionResult<TResult>.Completed(value);
        }
        catch (OperationCanceledException)
            when (linkedCancellationTokenSource.IsCancellationRequested)
        {
            return InteractionResult<TResult>.Canceled();
        }
        catch (Exception exception)
        {
            return InteractionResult<TResult>.Failed(exception);
        }
    }

    private void Remove(HandlerRegistration registration)
    {
        lock (_gate)
        {
            _handlers.Remove(registration);
        }
    }

    private sealed class HandlerRegistration : IDisposable
    {
        private readonly Interaction<TRequest, TResult> _interaction;
        private bool _disposed;

        public HandlerRegistration(
            Interaction<TRequest, TResult> interaction,
            Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
            IActivationScope? activationScope)
        {
            _interaction = interaction;
            Handler = handler;
            ActivationScope = activationScope;
        }

        public Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> Handler { get; }

        public IActivationScope? ActivationScope { get; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _interaction.Remove(this);
        }
    }
}
