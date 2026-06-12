using AtomUI.City.Generators.Diagnostics;
using AtomUI.City.Generators.Routing;

namespace AtomUI.City.Generators.Tests;

public sealed class RouteManifestBuilderTests
{
    [Fact]
    public void BuildSortsRoutesDeterministicallyAndResolvesParentRouteIds()
    {
        var result = RouteManifestBuilder.Build(
            [
                Route("Sample.App.AppRoutes", "Settings", "app.settings", RouteDefinitionMetadataKind.Route, "settings", parentMethodName: "Shell"),
                Route("Sample.App.AppRoutes", "Shell", "app.shell", RouteDefinitionMetadataKind.Layout, null),
            ]);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(["app.settings", "app.shell"], result.Manifest.Routes.Select(route => route.Id));
        Assert.Equal("app.shell", result.Manifest.Routes[0].ParentRouteId);
    }

    [Fact]
    public void BuildReportsDuplicateRouteIds()
    {
        var result = RouteManifestBuilder.Build(
            [
                Route("Sample.App.AppRoutes", "Settings", "app.settings", RouteDefinitionMetadataKind.Route, "settings"),
                Route("Sample.App.AdminRoutes", "Settings", "app.settings", RouteDefinitionMetadataKind.Route, "admin/settings"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.DuplicateRoute, diagnostic.Id);
        Assert.Empty(result.Manifest.Routes);
    }

    [Fact]
    public void BuildReportsMissingParentRoutes()
    {
        var result = RouteManifestBuilder.Build(
            [
                Route("Sample.App.AppRoutes", "Settings", "app.settings", RouteDefinitionMetadataKind.Route, "settings", parentMethodName: "Shell"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Routes);
    }

    [Fact]
    public void BuildReportsSiblingTemplateConflicts()
    {
        var result = RouteManifestBuilder.Build(
            [
                Route("Sample.App.AppRoutes", "Settings", "app.settings", RouteDefinitionMetadataKind.Route, "settings", parentMethodName: "Shell"),
                Route("Sample.App.AppRoutes", "Profile", "app.profile", RouteDefinitionMetadataKind.Route, "settings", parentMethodName: "Shell"),
                Route("Sample.App.AppRoutes", "Shell", "app.shell", RouteDefinitionMetadataKind.Layout, null),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.DuplicateRoute, diagnostic.Id);
        Assert.Empty(result.Manifest.Routes);
    }

    [Fact]
    public void BuildCarriesRouteLocalizationMetadata()
    {
        var result = RouteManifestBuilder.Build(
            [
                new RouteDefinitionMetadata(
                    "Sample.App.AppRoutes",
                    "Settings",
                    "app.settings",
                    RouteDefinitionMetadataKind.Route,
                    "settings",
                    "Sample.App.SettingsViewModel",
                    parentMethodName: null,
                    outletName: "primary",
                    extensionPoint: null,
                    redirectTargetMethodName: null,
                    titleKey: "Routes.Settings.Title",
                    descriptionKey: "Routes.Settings.Description",
                    breadcrumbKey: "Routes.Settings.Breadcrumb",
                    groupKey: "Routes.Settings.Group",
                    errorTitleKey: "Routes.Settings.ErrorTitle"),
            ]);

        var route = Assert.Single(result.Manifest.Routes);

        Assert.Equal("Routes.Settings.Title", route.TitleKey);
        Assert.Equal("Routes.Settings.Description", route.DescriptionKey);
        Assert.Equal("Routes.Settings.Breadcrumb", route.BreadcrumbKey);
        Assert.Equal("Routes.Settings.Group", route.GroupKey);
        Assert.Equal("Routes.Settings.ErrorTitle", route.ErrorTitleKey);
    }

    private static RouteDefinitionMetadata Route(
        string routeMapTypeName,
        string methodName,
        string id,
        RouteDefinitionMetadataKind kind,
        string? template,
        string? parentMethodName = null)
    {
        return new RouteDefinitionMetadata(
            routeMapTypeName,
            methodName,
            id,
            kind,
            template,
            viewModelTypeName: "Sample.App." + methodName + "ViewModel",
            parentMethodName,
            outletName: "primary",
            extensionPoint: null,
            redirectTargetMethodName: null);
    }
}
