using AtomUI.City.Generators.DependencyInjection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace AtomUI.City.Generators.Tests;

public sealed class ServiceRegistrationMetadataReaderTests
{
    [Fact]
    public void TryReadReadsServiceAndExposeServicesAttributes()
    {
        var registration = ReadSingleRegistration(
            """
            using AtomUI.City.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace Sample.App;

            public interface IClock
            {
            }

            [Service(ServiceLifetime.Singleton)]
            [ExposeServices(typeof(IClock))]
            public sealed class SystemClock : IClock
            {
            }
            """,
            "Sample.App.SystemClock");

        Assert.Equal("Sample.App.SystemClock", registration.ImplementationTypeName);
        Assert.Equal(ServiceRegistrationLifetime.Singleton, registration.Lifetime);
        Assert.Equal(["Sample.App.IClock"], registration.ExposedServiceTypeNames);
    }

    [Fact]
    public void TryReadReadsScopedServiceAttribute()
    {
        var registration = ReadSingleRegistration(
            """
            using AtomUI.City.DependencyInjection;

            namespace Sample.App;

            public interface IUserSession
            {
            }

            [ScopedService(typeof(IUserSession))]
            public sealed class UserSession : IUserSession
            {
            }
            """,
            "Sample.App.UserSession");

        Assert.Equal(ServiceRegistrationLifetime.Scoped, registration.Lifetime);
        Assert.Equal(["Sample.App.IUserSession"], registration.ExposedServiceTypeNames);
    }

    [Fact]
    public void TryReadReadsDependencyMarkerInterfaces()
    {
        var registration = ReadSingleRegistration(
            """
            using AtomUI.City.DependencyInjection;

            namespace Sample.App;

            public sealed class CacheStore : ISingletonDependency
            {
            }
            """,
            "Sample.App.CacheStore");

        Assert.Equal(ServiceRegistrationLifetime.Singleton, registration.Lifetime);
        Assert.Equal(["Sample.App.CacheStore"], registration.ExposedServiceTypeNames);
    }

    [Fact]
    public void TryReadReturnsNullForNonServiceTypes()
    {
        var compilation = CreateCompilation(
            """
            namespace Sample.App;

            public sealed class PlainType
            {
            }
            """);
        var type = GetTypeSymbol(compilation, "Sample.App.PlainType");

        Assert.Null(ServiceRegistrationMetadataReader.TryRead(type));
    }

    private static ServiceRegistrationMetadata ReadSingleRegistration(string source, string typeName)
    {
        var compilation = CreateCompilation(source);
        var type = GetTypeSymbol(compilation, typeName);
        var registration = ServiceRegistrationMetadataReader.TryRead(type);

        Assert.NotNull(registration);

        return registration;
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Concat(
                [
                    MetadataReference.CreateFromFile(typeof(AtomUI.City.Modularity.ModuleBase).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(ServiceLifetime).Assembly.Location),
                ])
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
            .OfType<ClassDeclarationSyntax>()
            .Single(type => string.Equals(type.Identifier.ValueText, typeName.Split('.').Last(), StringComparison.Ordinal));
        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        return semanticModel.GetDeclaredSymbol(declaration)!;
    }
}
