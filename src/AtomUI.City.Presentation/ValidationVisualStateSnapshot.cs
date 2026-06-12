using AtomUI.City.Mvvm;

namespace AtomUI.City.Presentation;

public sealed class ValidationVisualStateSnapshot
{
    private ValidationVisualStateSnapshot(
        ValidationStatus status,
        IReadOnlyDictionary<string, IReadOnlyList<string>> errors,
        IReadOnlyDictionary<string, IReadOnlyList<ValidationMessage>> messages,
        Exception? exception)
    {
        Status = status;
        Errors = errors;
        Messages = messages;
        Exception = exception;
    }

    public ValidationStatus Status { get; }

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Errors { get; }

    public IReadOnlyDictionary<string, IReadOnlyList<ValidationMessage>> Messages { get; }

    public Exception? Exception { get; }

    public static ValidationVisualStateSnapshot From(ValidationScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return new ValidationVisualStateSnapshot(
            scope.Status,
            Copy(scope.Errors),
            Copy(scope.Messages),
            scope.Exception);
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<T>> Copy<T>(
        IReadOnlyDictionary<string, IReadOnlyList<T>> values)
    {
        var copy = new Dictionary<string, IReadOnlyList<T>>(StringComparer.Ordinal);

        foreach (var item in values)
        {
            copy[item.Key] = item.Value.ToArray();
        }

        return copy;
    }
}
