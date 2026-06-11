namespace AtomUI.City.Localization;

public sealed class LocalizationResult
{
    private LocalizationResult(LocalizationError? error)
    {
        Error = error;
    }

    public LocalizationError? Error { get; }

    public bool Succeeded => Error is null;

    public static LocalizationResult Success()
    {
        return new LocalizationResult(error: null);
    }

    public static LocalizationResult Failed(LocalizationError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new LocalizationResult(error);
    }
}
