using AtomUI.City.Mvvm;

namespace AtomUI.City.Presentation;

public interface IInteractionHandlerRegistry
{
    IDisposable Register<TRequest, TResult>(
        Func<InteractionContext<TRequest>, CancellationToken, ValueTask<TResult>> handler,
        IActivationScope? activationScope = null);

    ValueTask<InteractionResult<TResult>> HandleAsync<TRequest, TResult>(
        TRequest request,
        CancellationToken cancellationToken = default);
}
