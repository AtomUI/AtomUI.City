namespace AtomUI.City.State;

public interface IStateValue<out T>
{
    T Value { get; }
}
