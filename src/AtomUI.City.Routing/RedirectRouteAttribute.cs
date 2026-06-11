namespace AtomUI.City.Routing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RedirectRouteAttribute : RouteDefinitionAttribute
{
    public RedirectRouteAttribute(string template)
        : base(RouteDefinitionKind.Redirect, template, null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
    }
}
