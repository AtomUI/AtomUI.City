namespace AtomUI.City.State;

public readonly record struct StateKey<T>
{
    public StateKey(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    public string Name { get; }

    public override string ToString() => Name;
}
