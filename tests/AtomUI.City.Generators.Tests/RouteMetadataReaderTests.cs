using AtomUI.City.Generators.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtomUI.City.Generators.Tests;

public sealed class RouteMetadataReaderTests
{
    [Fact]
    public void TryReadReturnsNullForNonRouteMapTypes()
    {
        var compilation = CreateCompilation(
            """
            namespace Sample.App;

            public static class PlainRoutes
            {
            }
            """);
        var type = GetTypeSymbol(compilation, "Sample.App.PlainRoutes");

        Assert.Null(RouteMetadataReader.TryRead(type));
    }

    [Fact]
    public void TryReadUsesRouteMapTypeAndMethodNameAsDefaultRouteId()
    {
        var routeMap = ReadRouteMap(
            """
            using AtomUI.City.Routing;

            namespace Sample.App;

            public sealed class ShellViewModel
            {
            }

            [RouteMap]
            public static partial class AppRoutes
            {
                [LayoutRoute(typeof(ShellViewModel))]
                public static RouteReference Shell() => default;
            }
            """);

        var route = Assert.Single(routeMap.Routes);

        Assert.Equal("Sample.App.AppRoutes", routeMap.TypeName);
        Assert.Equal("Sample.App.AppRoutes.Shell", route.Id);
        Assert.Equal(RouteDefinitionMetadataKind.Layout, route.Kind);
        Assert.Equal("Sample.App.ShellViewModel", route.ViewModelTypeName);
    }

    [Fact]
    public void TryReadReadsRouteMetadataFromRouteMethods()
    {
        var routeMap = ReadRouteMap(
            """
            using AtomUI.City.Routing;

            namespace Sample.App;

            public sealed class ShellViewModel
            {
            }

            public sealed class SettingsViewModel
            {
            }

            [RouteMap]
            public static partial class AppRoutes
            {
                [LayoutRoute(typeof(ShellViewModel), Id = "app.shell")]
                public static RouteReference Shell() => default;

                [Route("settings", typeof(SettingsViewModel), Id = "app.settings", Parent = nameof(Shell), Outlet = "side",
                    TitleKey = "Routes.Settings.Title",
                    DescriptionKey = "Routes.Settings.Description",
                    BreadcrumbKey = "Routes.Settings.Breadcrumb",
                    GroupKey = "Routes.Settings.Group",
                    ErrorTitleKey = "Routes.Settings.ErrorTitle")]
                public static RouteReference Settings() => default;
            }
            """);

        Assert.Collection(
            routeMap.Routes,
            route =>
            {
                Assert.Equal("app.shell", route.Id);
                Assert.Equal(RouteDefinitionMetadataKind.Layout, route.Kind);
            },
            route =>
            {
                Assert.Equal("app.settings", route.Id);
                Assert.Equal(RouteDefinitionMetadataKind.Route, route.Kind);
                Assert.Equal("settings", route.Template);
                Assert.Equal("Sample.App.SettingsViewModel", route.ViewModelTypeName);
                Assert.Equal("Shell", route.ParentMethodName);
                Assert.Equal("side", route.OutletName);
                Assert.Equal("Routes.Settings.Title", route.TitleKey);
                Assert.Equal("Routes.Settings.Description", route.DescriptionKey);
                Assert.Equal("Routes.Settings.Breadcrumb", route.BreadcrumbKey);
                Assert.Equal("Routes.Settings.Group", route.GroupKey);
                Assert.Equal("Routes.Settings.ErrorTitle", route.ErrorTitleKey);
            });
    }

    private static RouteMapMetadata ReadRouteMap(string source, string typeName = "Sample.App.AppRoutes")
    {
        var compilation = CreateCompilation(source);
        var type = GetTypeSymbol(compilation, typeName);
        var routeMap = RouteMetadataReader.TryRead(type);

        Assert.NotNull(routeMap);

        return routeMap;
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat([MetadataReference.CreateFromFile(typeof(AtomUI.City.Routing.RouteReference).Assembly.Location)])
            .DistinctBy(reference => reference.Display)
            .ToArray();

        return CSharpCompilation.Create(
            "Sample.App",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static INamedTypeSymbol GetTypeSymbol(Compilation compilation, string typeName)
    {
        var syntaxTree = compilation.SyntaxTrees.Single();
        var declaration = syntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Single(type => string.Equals(type.Identifier.ValueText, typeName.Split('.').Last(), StringComparison.Ordinal));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        return (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(declaration)!;
    }
}
