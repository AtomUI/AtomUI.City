namespace AtomUI.City.Presentation;

public interface IRouteOutlet
{
    string Name { get; }

    object? CurrentContent { get; }

    ValueTask<RouteOutletCommitResult> CommitAsync(
        RouteOutletCommitPlan plan,
        CancellationToken cancellationToken = default);
}
