namespace AtomUI.City.State;

public interface IApplicationStateWriter
{
    IWritableState<T> GetWritable<T>(StateKey<T> key);

    bool Set<T>(StateKey<T> key, T value);

    bool Update<T>(StateKey<T> key, Func<T, T> updater);
}
