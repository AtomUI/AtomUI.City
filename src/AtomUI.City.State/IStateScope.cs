namespace AtomUI.City.State;

public interface IStateScope : IDisposable
{
    string Id { get; }

    StateScopeState State { get; }

    void Add(IDisposable subscription);
}
