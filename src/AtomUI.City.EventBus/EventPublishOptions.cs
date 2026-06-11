namespace AtomUI.City.EventBus;

public sealed class EventPublishOptions
{
    public static EventPublishOptions Default { get; } = new();

    public string? CorrelationId { get; init; }

    public string? CausationId { get; init; }

    public int PublishDepth { get; init; }
}
