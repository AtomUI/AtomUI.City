using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Routing;

public static class RouteManifestBuilder
{
    public static RouteManifestResult Build(IReadOnlyList<RouteDefinitionMetadata> routes)
    {
        if (routes is null)
        {
            throw new ArgumentNullException(nameof(routes));
        }

        var diagnostics = new List<GeneratorDiagnostic>();
        var routesById = new Dictionary<string, RouteDefinitionMetadata>(StringComparer.Ordinal);
        var routesByMethod = new Dictionary<string, RouteDefinitionMetadata>(StringComparer.Ordinal);

        foreach (var route in routes)
        {
            if (routesById.ContainsKey(route.Id))
            {
                diagnostics.Add(new GeneratorDiagnostic(
                    GeneratorDiagnostics.DuplicateRoute,
                    $"Route id '{route.Id}' is declared more than once.",
                    route.Id));
            }
            else
            {
                routesById.Add(route.Id, route);
            }

            var methodKey = CreateMethodKey(route.RouteMapTypeName, route.MethodName);

            if (!routesByMethod.ContainsKey(methodKey))
            {
                routesByMethod.Add(methodKey, route);
            }
        }

        var parentRouteIds = new Dictionary<string, string?>(StringComparer.Ordinal);
        var redirectTargetRouteIds = new Dictionary<string, string?>(StringComparer.Ordinal);

        foreach (var route in routes)
        {
            parentRouteIds[route.Id] = ResolveReferencedRouteId(route, route.ParentMethodName, routesByMethod, diagnostics, "parent");
            redirectTargetRouteIds[route.Id] = ResolveReferencedRouteId(route, route.RedirectTargetMethodName, routesByMethod, diagnostics, "redirect target");
        }

        DetectSiblingTemplateConflicts(routes, parentRouteIds, diagnostics);

        if (diagnostics.Count > 0)
        {
            return new RouteManifestResult(new RouteManifest([]), diagnostics);
        }

        var manifestRoutes = routes
            .Select(route => new RouteManifestRoute(
                route.Id,
                route.Kind,
                route.Template,
                route.ViewModelTypeName,
                parentRouteIds[route.Id],
                route.OutletName,
                route.ExtensionPoint,
                redirectTargetRouteIds[route.Id],
                route.TitleKey,
                route.DescriptionKey,
                route.BreadcrumbKey,
                route.GroupKey,
                route.ErrorTitleKey))
            .OrderBy(route => route.Id, StringComparer.Ordinal)
            .ToArray();

        return new RouteManifestResult(new RouteManifest(manifestRoutes), diagnostics);
    }

    private static string? ResolveReferencedRouteId(
        RouteDefinitionMetadata route,
        string? referencedMethodName,
        IReadOnlyDictionary<string, RouteDefinitionMetadata> routesByMethod,
        ICollection<GeneratorDiagnostic> diagnostics,
        string referenceKind)
    {
        if (string.IsNullOrWhiteSpace(referencedMethodName))
        {
            return null;
        }

        var referencedMethodKey = CreateMethodKey(route.RouteMapTypeName, referencedMethodName!);

        if (routesByMethod.TryGetValue(referencedMethodKey, out var referencedRoute))
        {
            return referencedRoute.Id;
        }

        diagnostics.Add(new GeneratorDiagnostic(
            GeneratorDiagnostics.InvalidManifestInput,
            $"Route '{route.Id}' references missing {referenceKind} route method '{referencedMethodName}'.",
            route.Id));

        return null;
    }

    private static void DetectSiblingTemplateConflicts(
        IEnumerable<RouteDefinitionMetadata> routes,
        IReadOnlyDictionary<string, string?> parentRouteIds,
        ICollection<GeneratorDiagnostic> diagnostics)
    {
        var templates = new Dictionary<string, RouteDefinitionMetadata>(StringComparer.Ordinal);

        foreach (var route in routes)
        {
            if (string.IsNullOrWhiteSpace(route.Template))
            {
                continue;
            }

            parentRouteIds.TryGetValue(route.Id, out var parentRouteId);

            var key = string.Join(
                "|",
                parentRouteId ?? string.Empty,
                route.OutletName,
                route.Template);

            if (!templates.TryGetValue(key, out _))
            {
                templates.Add(key, route);
                continue;
            }

            diagnostics.Add(new GeneratorDiagnostic(
                GeneratorDiagnostics.DuplicateRoute,
                $"Route template '{route.Template}' is declared more than once for the same parent and outlet.",
                route.Id));
        }
    }

    private static string CreateMethodKey(string routeMapTypeName, string methodName)
    {
        return routeMapTypeName + "." + methodName;
    }
}
