namespace AtomUI.City.State;

public interface IStateRegistry
{
    void Add<T>(StateDefinition<T> definition);
}
