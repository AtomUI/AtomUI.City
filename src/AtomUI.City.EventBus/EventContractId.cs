namespace AtomUI.City.EventBus;

public readonly record struct EventContractId
{
    public EventContractId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
