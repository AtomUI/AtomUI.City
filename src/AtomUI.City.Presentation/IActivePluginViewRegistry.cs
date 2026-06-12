namespace AtomUI.City.Presentation;

public interface IActivePluginViewRegistry
{
    IReadOnlyList<ActivePluginView> ActiveViews { get; }

    IActivePluginViewLease Track(ActivePluginView view);

    ValueTask<int> ClosePluginViewsAsync(
        string pluginId,
        CancellationToken cancellationToken = default);

    ValueTask<int> CloseContributionViewsAsync(
        string contributionId,
        CancellationToken cancellationToken = default);
}
