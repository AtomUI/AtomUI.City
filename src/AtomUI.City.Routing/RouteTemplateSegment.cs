namespace AtomUI.City.Routing;

public sealed class RouteTemplateSegment
{
    private RouteTemplateSegment(
        RouteTemplateSegmentKind kind,
        string? literal,
        string? name,
        bool isOptional,
        string? defaultValue,
        IReadOnlyList<string> constraints)
    {
        Kind = kind;
        Literal = literal;
        Name = name;
        IsOptional = isOptional;
        DefaultValue = defaultValue;
        Constraints = constraints;
    }

    public RouteTemplateSegmentKind Kind { get; }

    public string? Literal { get; }

    public string? Name { get; }

    public bool IsOptional { get; }

    public string? DefaultValue { get; }

    public IReadOnlyList<string> Constraints { get; }

    internal static RouteTemplateSegment LiteralSegment(string literal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(literal);

        return new RouteTemplateSegment(RouteTemplateSegmentKind.Literal, literal, null, false, null, []);
    }

    internal static RouteTemplateSegment ParameterSegment(
        RouteTemplateSegmentKind kind,
        string name,
        bool isOptional,
        string? defaultValue,
        IReadOnlyList<string> constraints)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(constraints);

        return new RouteTemplateSegment(kind, null, name, isOptional, defaultValue, constraints);
    }
}
