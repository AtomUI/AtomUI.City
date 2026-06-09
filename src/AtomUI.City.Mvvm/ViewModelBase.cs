using CommunityToolkit.Mvvm.ComponentModel;

namespace AtomUI.City.Mvvm;

public abstract class ViewModelBase : ObservableValidator, IActivatable
{
    public async ValueTask ActivateAsync(IActivationScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        await OnActivatedAsync(scope).ConfigureAwait(false);
    }

    public async ValueTask DeactivateAsync()
    {
        await OnDeactivatedAsync().ConfigureAwait(false);
    }

    protected virtual ValueTask OnActivatedAsync(IActivationScope scope) => ValueTask.CompletedTask;

    protected virtual ValueTask OnDeactivatedAsync() => ValueTask.CompletedTask;
}
