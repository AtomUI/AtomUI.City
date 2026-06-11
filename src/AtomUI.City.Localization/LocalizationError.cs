namespace AtomUI.City.Localization;

public sealed record LocalizationError(
    LocalizationErrorKind Kind,
    string Message,
    Exception? Exception = null);
