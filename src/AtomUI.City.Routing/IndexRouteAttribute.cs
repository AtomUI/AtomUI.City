namespace AtomUI.City.Routing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class IndexRouteAttribute : RouteDefinitionAttribute
{
    public IndexRouteAttribute(Type viewModelType)
        : base(RouteDefinitionKind.Index, null, viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
    }
}
