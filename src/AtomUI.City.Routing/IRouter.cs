namespace AtomUI.City.Routing;

public interface IRouter
{
    ValueTask<NavigationResult> NavigateAsync(
        RouteReference route,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> NavigateAsync<TParameters>(
        RouteReference<TParameters> route,
        TParameters parameters,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> NavigateByPathAsync(
        string path,
        NavigationOptions? options = null,
        CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> BackAsync(CancellationToken cancellationToken = default);

    ValueTask<NavigationResult> ForwardAsync(CancellationToken cancellationToken = default);
}
