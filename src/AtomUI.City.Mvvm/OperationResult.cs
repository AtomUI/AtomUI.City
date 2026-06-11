namespace AtomUI.City.Mvvm;

public sealed record OperationResult(
    OperationStatus Status,
    TimeSpan Elapsed,
    Exception? Error = null);
