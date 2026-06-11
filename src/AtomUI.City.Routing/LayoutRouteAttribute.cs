namespace AtomUI.City.Routing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class LayoutRouteAttribute : RouteDefinitionAttribute
{
    public LayoutRouteAttribute(Type viewModelType)
        : base(RouteDefinitionKind.Layout, null, viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
    }
}
