using System.Globalization;

namespace AtomUI.City.Routing;

public sealed class RouteTemplate
{
    private RouteTemplate(string pattern, IReadOnlyList<RouteTemplateSegment> segments)
    {
        Pattern = pattern;
        Segments = Array.AsReadOnly(segments.ToArray());
    }

    public string Pattern { get; }

    public IReadOnlyList<RouteTemplateSegment> Segments { get; }

    public static RouteTemplate Parse(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern.Length > 0 && string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("The value cannot be composed entirely of whitespace.", nameof(pattern));
        }

        var normalizedPattern = NormalizePattern(pattern);
        var segments = normalizedPattern.Length == 0
            ? []
            : normalizedPattern
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseSegment)
                .ToArray();

        return new RouteTemplate(normalizedPattern, segments);
    }

    public bool TryMatch(string path, out IReadOnlyDictionary<string, string> values)
    {
        var routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var pathSegments = NormalizePattern(path)
            .Split('/', StringSplitOptions.RemoveEmptyEntries);
        var pathIndex = 0;

        foreach (var segment in Segments)
        {
            if (segment.Kind == RouteTemplateSegmentKind.CatchAll)
            {
                routeValues[segment.Name!] = string.Join('/', pathSegments.Skip(pathIndex));
                pathIndex = pathSegments.Length;
                continue;
            }

            if (pathIndex >= pathSegments.Length)
            {
                if (segment.Kind == RouteTemplateSegmentKind.Parameter && segment.DefaultValue is not null)
                {
                    routeValues[segment.Name!] = segment.DefaultValue;
                    continue;
                }

                if (segment.Kind == RouteTemplateSegmentKind.Parameter && segment.IsOptional)
                {
                    continue;
                }

                values = EmptyValues.Instance;

                return false;
            }

            var pathSegment = pathSegments[pathIndex];

            if (segment.Kind == RouteTemplateSegmentKind.Literal)
            {
                if (!string.Equals(segment.Literal, pathSegment, StringComparison.OrdinalIgnoreCase))
                {
                    values = EmptyValues.Instance;

                    return false;
                }

                pathIndex++;
                continue;
            }

            if (!SatisfiesConstraints(pathSegment, segment.Constraints))
            {
                values = EmptyValues.Instance;

                return false;
            }

            routeValues[segment.Name!] = pathSegment;
            pathIndex++;
        }

        if (pathIndex != pathSegments.Length)
        {
            values = EmptyValues.Instance;

            return false;
        }

        values = routeValues;

        return true;
    }

    internal int SpecificityScore()
    {
        return Segments.Sum(segment => segment.Kind switch
        {
            RouteTemplateSegmentKind.Literal => 40,
            RouteTemplateSegmentKind.Parameter when segment.Constraints.Count > 0 => 30,
            RouteTemplateSegmentKind.Parameter when segment.IsOptional || segment.DefaultValue is not null => 10,
            RouteTemplateSegmentKind.Parameter => 20,
            RouteTemplateSegmentKind.CatchAll => 0,
            _ => 0,
        });
    }

    private static RouteTemplateSegment ParseSegment(string segment)
    {
        if (!segment.StartsWith('{') || !segment.EndsWith('}'))
        {
            return RouteTemplateSegment.LiteralSegment(segment);
        }

        var body = segment[1..^1];
        var kind = RouteTemplateSegmentKind.Parameter;

        if (body.StartsWith('*'))
        {
            kind = RouteTemplateSegmentKind.CatchAll;
            body = body[1..];
        }

        string? defaultValue = null;
        var defaultSeparatorIndex = body.IndexOf('=', StringComparison.Ordinal);

        if (defaultSeparatorIndex >= 0)
        {
            defaultValue = body[(defaultSeparatorIndex + 1)..];
            body = body[..defaultSeparatorIndex];
        }

        var parts = body.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            throw new RouteGraphException(RouteGraphError.InvalidRouteTemplate, "Route parameter name cannot be empty.");
        }

        var name = parts[0];
        var isOptional = name.EndsWith("?", StringComparison.Ordinal);

        if (isOptional)
        {
            name = name[..^1];
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new RouteGraphException(RouteGraphError.InvalidRouteTemplate, "Route parameter name cannot be empty.");
        }

        return RouteTemplateSegment.ParameterSegment(
            kind,
            name,
            isOptional,
            defaultValue,
            parts.Skip(1).ToArray());
    }

    private static bool SatisfiesConstraints(string value, IReadOnlyList<string> constraints)
    {
        foreach (var constraint in constraints)
        {
            if (!SatisfiesConstraint(value, constraint))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SatisfiesConstraint(string value, string constraint)
    {
        return constraint switch
        {
            "bool" => bool.TryParse(value, out _),
            "datetime" => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
            "decimal" => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _),
            "double" => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _),
            "float" => float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _),
            "guid" => Guid.TryParse(value, out _),
            "int" => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            "long" => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
            "alpha" => value.All(char.IsLetter),
            _ => true,
        };
    }

    private static string NormalizePattern(string pattern)
    {
        return pattern.Trim().Trim('/');
    }

    private sealed class EmptyValues : Dictionary<string, string>
    {
        public static readonly EmptyValues Instance = new();

        private EmptyValues()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
