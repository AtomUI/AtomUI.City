using Microsoft.CodeAnalysis;

namespace AtomUI.City.Generators.Routing;

public static class RouteMetadataReader
{
    private const string IndexRouteAttributeName = "AtomUI.City.Routing.IndexRouteAttribute";
    private const string LayoutRouteAttributeName = "AtomUI.City.Routing.LayoutRouteAttribute";
    private const string RedirectRouteAttributeName = "AtomUI.City.Routing.RedirectRouteAttribute";
    private const string RouteAttributeName = "AtomUI.City.Routing.RouteAttribute";
    private const string RouteExtensionPointAttributeName = "AtomUI.City.Routing.RouteExtensionPointAttribute";
    private const string RouteGroupAttributeName = "AtomUI.City.Routing.RouteGroupAttribute";
    private const string RouteMapAttributeName = "AtomUI.City.Routing.RouteMapAttribute";

    public static RouteMapMetadata? TryRead(INamedTypeSymbol type)
    {
        if (!HasAttribute(type, RouteMapAttributeName))
        {
            return null;
        }

        var routeMapTypeName = GetTypeName(type);
        var routes = type
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Select(method => TryReadRoute(routeMapTypeName, method))
            .Where(route => route is not null)
            .Cast<RouteDefinitionMetadata>()
            .ToArray();

        return new RouteMapMetadata(routeMapTypeName, routes);
    }

    private static RouteDefinitionMetadata? TryReadRoute(string routeMapTypeName, IMethodSymbol method)
    {
        foreach (var attribute in method.GetAttributes())
        {
            var attributeName = GetAttributeTypeName(attribute);
            var kind = TryReadKind(attributeName);

            if (kind is null)
            {
                continue;
            }

            var id = ReadNamedString(attribute, "Id") ?? routeMapTypeName + "." + method.Name;

            return new RouteDefinitionMetadata(
                routeMapTypeName,
                method.Name,
                id,
                kind.Value,
                ReadTemplate(attribute, kind.Value),
                ReadViewModelTypeName(attribute, kind.Value),
                ReadNamedString(attribute, "Parent"),
                ReadNamedString(attribute, "Outlet") ?? "primary",
                ReadExtensionPoint(attribute, kind.Value),
                ReadNamedString(attribute, "Target"),
                ReadNamedString(attribute, "TitleKey"),
                ReadNamedString(attribute, "DescriptionKey"),
                ReadNamedString(attribute, "BreadcrumbKey"),
                ReadNamedString(attribute, "GroupKey"),
                ReadNamedString(attribute, "ErrorTitleKey"));
        }

        return null;
    }

    private static RouteDefinitionMetadataKind? TryReadKind(string? attributeName)
    {
        switch (attributeName)
        {
            case RouteAttributeName:
                return RouteDefinitionMetadataKind.Route;
            case LayoutRouteAttributeName:
                return RouteDefinitionMetadataKind.Layout;
            case IndexRouteAttributeName:
                return RouteDefinitionMetadataKind.Index;
            case RouteGroupAttributeName:
                return RouteDefinitionMetadataKind.Group;
            case RedirectRouteAttributeName:
                return RouteDefinitionMetadataKind.Redirect;
            case RouteExtensionPointAttributeName:
                return RouteDefinitionMetadataKind.ExtensionPoint;
            default:
                return null;
        }
    }

    private static string? ReadTemplate(AttributeData attribute, RouteDefinitionMetadataKind kind)
    {
        if (kind is RouteDefinitionMetadataKind.Layout or RouteDefinitionMetadataKind.Index or RouteDefinitionMetadataKind.ExtensionPoint)
        {
            return null;
        }

        return ReadConstructorString(attribute, 0);
    }

    private static string? ReadViewModelTypeName(AttributeData attribute, RouteDefinitionMetadataKind kind)
    {
        var argumentIndex = kind == RouteDefinitionMetadataKind.Route ? 1 : 0;

        if (kind is RouteDefinitionMetadataKind.Group or RouteDefinitionMetadataKind.Redirect or RouteDefinitionMetadataKind.ExtensionPoint)
        {
            return null;
        }

        return ReadConstructorTypeName(attribute, argumentIndex);
    }

    private static string? ReadExtensionPoint(AttributeData attribute, RouteDefinitionMetadataKind kind)
    {
        if (kind == RouteDefinitionMetadataKind.ExtensionPoint)
        {
            return ReadConstructorString(attribute, 0);
        }

        return ReadNamedString(attribute, "ExtensionPoint");
    }

    private static string? ReadConstructorString(AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments.Length <= index)
        {
            return null;
        }

        return attribute.ConstructorArguments[index].Value as string;
    }

    private static string? ReadConstructorTypeName(AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments.Length <= index)
        {
            return null;
        }

        return attribute.ConstructorArguments[index].Value is INamedTypeSymbol type ? GetTypeName(type) : null;
    }

    private static bool HasAttribute(INamedTypeSymbol type, string attributeName)
    {
        return type
            .GetAttributes()
            .Any(attribute => string.Equals(GetAttributeTypeName(attribute), attributeName, StringComparison.Ordinal));
    }

    private static string? ReadNamedString(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                return argument.Value.Value as string;
            }
        }

        return null;
    }

    private static string? GetAttributeTypeName(AttributeData attribute)
    {
        return attribute.AttributeClass is null ? null : GetTypeName(attribute.AttributeClass);
    }

    private static string GetTypeName(INamedTypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }
}
