namespace AtomUI.City.Routing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RouteExtensionPointAttribute : RouteDefinitionAttribute
{
    public RouteExtensionPointAttribute(string extensionPoint)
        : base(RouteDefinitionKind.ExtensionPoint, null, null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extensionPoint);

        ExtensionPoint = extensionPoint;
    }
}
