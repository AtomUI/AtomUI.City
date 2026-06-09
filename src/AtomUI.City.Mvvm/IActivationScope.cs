namespace AtomUI.City.Mvvm;

public interface IActivationScope : IDisposable
{
    CancellationToken CancellationToken { get; }

    void Add(IDisposable disposable);
}
