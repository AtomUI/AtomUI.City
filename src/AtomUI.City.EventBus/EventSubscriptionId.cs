namespace AtomUI.City.EventBus;

public readonly record struct EventSubscriptionId
{
    public EventSubscriptionId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static EventSubscriptionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}
