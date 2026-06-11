namespace AtomUI.City.Mvvm;

public interface IConfirmDeactivate
{
    ValueTask<DeactivationResult> ConfirmDeactivateAsync(CancellationToken cancellationToken);
}
