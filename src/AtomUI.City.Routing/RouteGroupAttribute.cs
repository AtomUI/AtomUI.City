namespace AtomUI.City.Routing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RouteGroupAttribute : RouteDefinitionAttribute
{
    public RouteGroupAttribute(string template)
        : base(RouteDefinitionKind.Group, template, null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
    }
}
