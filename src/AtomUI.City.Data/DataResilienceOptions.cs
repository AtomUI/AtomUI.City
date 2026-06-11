namespace AtomUI.City.Data;

public sealed class DataResilienceOptions
{
    public TimeSpan? Timeout { get; init; }

    public int MaxRetryAttempts { get; init; }

    public bool AllowMutationRetry { get; init; }

    public static DataResilienceOptions None { get; } = new();
}
