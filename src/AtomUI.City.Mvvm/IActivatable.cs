namespace AtomUI.City.Mvvm;

public interface IActivatable
{
    ValueTask ActivateAsync(IActivationScope scope);

    ValueTask DeactivateAsync();
}
