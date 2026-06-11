namespace AtomUI.City.Mvvm;

public sealed class CommandExecutionState
{
    public bool IsExecuting { get; private set; }

    public OperationResult? LastResult { get; private set; }

    public Exception? LastError { get; private set; }

    public CancellationToken CancellationToken { get; private set; }

    internal void Begin(CancellationToken cancellationToken)
    {
        IsExecuting = true;
        LastResult = null;
        LastError = null;
        CancellationToken = cancellationToken;
    }

    internal void Complete(OperationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        LastResult = result;
        LastError = result.Error;
        IsExecuting = false;
    }
}
