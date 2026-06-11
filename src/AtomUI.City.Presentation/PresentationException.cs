namespace AtomUI.City.Presentation;

public sealed class PresentationException : Exception
{
    public PresentationException(PresentationError error, string message)
        : base(message)
    {
        Error = error;
    }

    public PresentationException(
        PresentationError error,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        Error = error;
    }

    public PresentationError Error { get; }
}
