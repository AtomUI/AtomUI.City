namespace AtomUI.City.Routing;

public sealed class RouteDescriptor
{
    public RouteDescriptor(
        string routeId,
        RouteDefinitionKind kind,
        string? template,
        ViewModelTargetDescriptor? viewModelTarget,
        string? parentRouteId = null,
        string outletName = "primary",
        string? extensionPoint = null,
        string? redirectTargetRouteId = null,
        IReadOnlyList<Type>? enterGuardTypes = null,
        IReadOnlyList<Type>? leaveGuardTypes = null,
        IReadOnlyList<Type>? matchPolicyTypes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(routeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outletName);

        RouteId = routeId;
        Kind = kind;
        Template = string.IsNullOrWhiteSpace(template) ? null : RouteTemplate.Parse(template);
        ViewModelTarget = viewModelTarget;
        ParentRouteId = parentRouteId;
        OutletName = outletName;
        ExtensionPoint = extensionPoint;
        RedirectTargetRouteId = redirectTargetRouteId;
        EnterGuardTypes = enterGuardTypes?.ToArray() ?? [];
        LeaveGuardTypes = leaveGuardTypes?.ToArray() ?? [];
        MatchPolicyTypes = matchPolicyTypes?.ToArray() ?? [];
    }

    public string RouteId { get; }

    public RouteDefinitionKind Kind { get; }

    public RouteTemplate? Template { get; }

    public ViewModelTargetDescriptor? ViewModelTarget { get; }

    public string? ParentRouteId { get; }

    public string OutletName { get; }

    public string? ExtensionPoint { get; }

    public string? RedirectTargetRouteId { get; }

    public IReadOnlyList<Type> EnterGuardTypes { get; }

    public IReadOnlyList<Type> LeaveGuardTypes { get; }

    public IReadOnlyList<Type> MatchPolicyTypes { get; }
}
