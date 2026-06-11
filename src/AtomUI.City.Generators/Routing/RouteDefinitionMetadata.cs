namespace AtomUI.City.Generators.Routing;

public sealed class RouteDefinitionMetadata
{
    public RouteDefinitionMetadata(
        string routeMapTypeName,
        string methodName,
        string id,
        RouteDefinitionMetadataKind kind,
        string? template,
        string? viewModelTypeName,
        string? parentMethodName,
        string outletName,
        string? extensionPoint,
        string? redirectTargetMethodName)
    {
        if (string.IsNullOrWhiteSpace(routeMapTypeName))
        {
            throw new ArgumentException("Route map type name cannot be empty.", nameof(routeMapTypeName));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Route method name cannot be empty.", nameof(methodName));
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Route id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(outletName))
        {
            throw new ArgumentException("Outlet name cannot be empty.", nameof(outletName));
        }

        RouteMapTypeName = routeMapTypeName;
        MethodName = methodName;
        Id = id;
        Kind = kind;
        Template = template;
        ViewModelTypeName = viewModelTypeName;
        ParentMethodName = parentMethodName;
        OutletName = outletName;
        ExtensionPoint = extensionPoint;
        RedirectTargetMethodName = redirectTargetMethodName;
    }

    public string RouteMapTypeName { get; }

    public string MethodName { get; }

    public string Id { get; }

    public RouteDefinitionMetadataKind Kind { get; }

    public string? Template { get; }

    public string? ViewModelTypeName { get; }

    public string? ParentMethodName { get; }

    public string OutletName { get; }

    public string? ExtensionPoint { get; }

    public string? RedirectTargetMethodName { get; }
}
