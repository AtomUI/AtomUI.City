namespace AtomUI.City.Generators.Routing;

public sealed class RouteManifestRoute
{
    public RouteManifestRoute(
        string id,
        RouteDefinitionMetadataKind kind,
        string? template,
        string? viewModelTypeName,
        string? parentRouteId,
        string outletName,
        string? extensionPoint,
        string? redirectTargetRouteId)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Route id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(outletName))
        {
            throw new ArgumentException("Outlet name cannot be empty.", nameof(outletName));
        }

        Id = id;
        Kind = kind;
        Template = template;
        ViewModelTypeName = viewModelTypeName;
        ParentRouteId = parentRouteId;
        OutletName = outletName;
        ExtensionPoint = extensionPoint;
        RedirectTargetRouteId = redirectTargetRouteId;
    }

    public string Id { get; }

    public RouteDefinitionMetadataKind Kind { get; }

    public string? Template { get; }

    public string? ViewModelTypeName { get; }

    public string? ParentRouteId { get; }

    public string OutletName { get; }

    public string? ExtensionPoint { get; }

    public string? RedirectTargetRouteId { get; }
}
