namespace AtomUI.City.Presentation;

public sealed record PresentationPluginUnloadError(
    PresentationPluginUnloadErrorKind Kind,
    string Message,
    Exception? Exception = null);
