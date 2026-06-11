namespace AtomUI.City.Presentation;

public sealed class RouteOutletCommitResult
{
    private RouteOutletCommitResult(
        bool succeeded,
        PresentationError? error,
        string? message)
    {
        Succeeded = succeeded;
        Error = error;
        Message = message;
    }

    public bool Succeeded { get; }

    public PresentationError? Error { get; }

    public string? Message { get; }

    public static RouteOutletCommitResult Success()
    {
        return new RouteOutletCommitResult(
            succeeded: true,
            error: null,
            message: null);
    }

    public static RouteOutletCommitResult Failed(
        PresentationError error,
        string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new RouteOutletCommitResult(
            succeeded: false,
            error,
            message);
    }
}
