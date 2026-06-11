using System.Diagnostics;

namespace AtomUI.City.Mvvm;

public sealed class OperationScope
{
    private readonly Stopwatch _stopwatch;

    private OperationScope(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
        _stopwatch = Stopwatch.StartNew();
    }

    public CancellationToken CancellationToken { get; }

    public static OperationScope Start(CancellationToken cancellationToken)
    {
        return new OperationScope(cancellationToken);
    }

    public OperationResult Complete()
    {
        _stopwatch.Stop();

        return new OperationResult(OperationStatus.Completed, _stopwatch.Elapsed);
    }

    public OperationResult Cancel()
    {
        _stopwatch.Stop();

        return new OperationResult(OperationStatus.Canceled, _stopwatch.Elapsed);
    }

    public OperationResult Fail(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        _stopwatch.Stop();

        return new OperationResult(OperationStatus.Failed, _stopwatch.Elapsed, exception);
    }
}
