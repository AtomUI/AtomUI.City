using CommunityToolkit.Mvvm.ComponentModel;

namespace AtomUI.City.Mvvm;

public abstract class ViewModelBase : ObservableValidator, IActivatable
{
    public ActivationState ActivationState { get; private set; } = ActivationState.Constructed;

    public bool IsActive => ActivationState == ActivationState.Active;

    public IActivationScope? CurrentActivationScope => ActivationContext?.Scope;

    public ActivationContext? ActivationContext { get; private set; }

    public async ValueTask ActivateAsync(IActivationScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        await ActivateAsync(new ActivationContext(scope)).ConfigureAwait(false);
    }

    public async ValueTask ActivateAsync(ActivationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (ActivationState == ActivationState.Disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }

        ActivationState = ActivationState.Activating;
        ActivationContext = context;

        await OnActivatedAsync(context).ConfigureAwait(false);

        ActivationState = ActivationState.Active;
    }

    public async ValueTask DeactivateAsync()
    {
        if (ActivationState is ActivationState.Deactivated or ActivationState.Constructed)
        {
            return;
        }

        ActivationState = ActivationState.Deactivating;

        try
        {
            await OnDeactivatedAsync().ConfigureAwait(false);
        }
        finally
        {
            ActivationContext?.Scope.Dispose();
            ActivationContext = null;
            ActivationState = ActivationState.Deactivated;
        }
    }

    protected virtual ValueTask OnActivatedAsync(ActivationContext context) => OnActivatedAsync(context.Scope);

    protected virtual ValueTask OnActivatedAsync(IActivationScope scope) => ValueTask.CompletedTask;

    protected virtual ValueTask OnDeactivatedAsync() => ValueTask.CompletedTask;
}
