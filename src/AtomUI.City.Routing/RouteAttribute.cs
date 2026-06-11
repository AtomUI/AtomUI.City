namespace AtomUI.City.Routing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RouteAttribute : RouteDefinitionAttribute
{
    public RouteAttribute(string template, Type viewModelType)
        : base(RouteDefinitionKind.Route, template, viewModelType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(viewModelType);
    }
}
