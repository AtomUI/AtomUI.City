namespace AtomUI.City.Presentation;

public sealed class ActivePluginView
{
    public ActivePluginView(
        string pluginId,
        IRouteOutlet outlet,
        BoundViewHandle handle,
        string? contributionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        ArgumentNullException.ThrowIfNull(outlet);
        ArgumentNullException.ThrowIfNull(handle);

        PluginId = pluginId;
        Outlet = outlet;
        Handle = handle;
        ContributionId = string.IsNullOrWhiteSpace(contributionId) ? null : contributionId;
    }

    public string PluginId { get; }

    public string? ContributionId { get; }

    public IRouteOutlet Outlet { get; }

    public BoundViewHandle Handle { get; }
}
