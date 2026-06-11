namespace AtomUI.City.Mvvm;

public sealed class InteractionResult<TResult>
{
    private InteractionResult(
        InteractionResultStatus status,
        TResult? value,
        Exception? exception)
    {
        Status = status;
        Value = value;
        Exception = exception;
    }

    public InteractionResultStatus Status { get; }

    public TResult? Value { get; }

    public Exception? Exception { get; }

    public static InteractionResult<TResult> Completed(TResult value)
    {
        return new InteractionResult<TResult>(
            InteractionResultStatus.Completed,
            value,
            exception: null);
    }

    public static InteractionResult<TResult> Canceled()
    {
        return new InteractionResult<TResult>(
            InteractionResultStatus.Canceled,
            value: default,
            exception: null);
    }

    public static InteractionResult<TResult> Failed(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new InteractionResult<TResult>(
            InteractionResultStatus.Failed,
            value: default,
            exception);
    }

    public static InteractionResult<TResult> NotHandled()
    {
        return new InteractionResult<TResult>(
            InteractionResultStatus.NotHandled,
            value: default,
            exception: null);
    }
}
