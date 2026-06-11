namespace AtomUI.City.Routing;

public abstract class RouteDefinitionAttribute : Attribute
{
    protected RouteDefinitionAttribute(RouteDefinitionKind kind, string? template, Type? viewModelType)
    {
        Kind = kind;
        Template = template;
        ViewModelType = viewModelType;
    }

    public RouteDefinitionKind Kind { get; }

    public string? Template { get; }

    public Type? ViewModelType { get; }

    public string? Id { get; set; }

    public string? Parent { get; set; }

    public string Outlet { get; set; } = "primary";

    public string? ExtensionPoint { get; set; }

    public string? Target { get; set; }
}
