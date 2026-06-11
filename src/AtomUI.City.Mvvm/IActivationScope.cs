namespace AtomUI.City.Mvvm;

public interface IActivationScope : IDisposable, IAsyncDisposable
{
    CancellationToken CancellationToken { get; }

    void Add(IDisposable disposable);

    void AddAsync(IAsyncDisposable disposable);
}
