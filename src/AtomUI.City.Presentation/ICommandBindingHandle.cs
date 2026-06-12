namespace AtomUI.City.Presentation;

public interface ICommandBindingHandle : IDisposable
{
    ValueTask RefreshAsync(CancellationToken cancellationToken = default);
}
