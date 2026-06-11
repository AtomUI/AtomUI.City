namespace AtomUI.City.Mvvm;

public interface ICanDeactivate
{
    ValueTask<DeactivationResult> CanDeactivateAsync(CancellationToken cancellationToken);
}
