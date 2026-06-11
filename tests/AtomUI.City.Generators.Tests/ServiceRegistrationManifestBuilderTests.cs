using AtomUI.City.Generators.DependencyInjection;
using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.Tests;

public sealed class ServiceRegistrationManifestBuilderTests
{
    [Fact]
    public void BuildSortsRegistrationsDeterministically()
    {
        var result = ServiceRegistrationManifestBuilder.Build(
            [
                Registration("Sample.App.UserSession", ServiceRegistrationLifetime.Scoped, "Sample.App.IUserSession"),
                Registration("Sample.App.SystemClock", ServiceRegistrationLifetime.Singleton, "Sample.App.IClock"),
            ]);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(
            ["Sample.App.SystemClock", "Sample.App.UserSession"],
            result.Manifest.Registrations.Select(registration => registration.ImplementationTypeName));
    }

    [Fact]
    public void BuildReportsDuplicateExposedServicesByDefault()
    {
        var result = ServiceRegistrationManifestBuilder.Build(
            [
                Registration("Sample.App.SystemClock", ServiceRegistrationLifetime.Singleton, "Sample.App.IClock"),
                Registration("Sample.App.CustomClock", ServiceRegistrationLifetime.Singleton, "Sample.App.IClock"),
            ]);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(GeneratorDiagnosticIds.InvalidManifestInput, diagnostic.Id);
        Assert.Empty(result.Manifest.Registrations);
    }

    private static ServiceRegistrationMetadata Registration(
        string implementationTypeName,
        ServiceRegistrationLifetime lifetime,
        params string[] exposedServiceTypeNames)
    {
        return new ServiceRegistrationMetadata(
            implementationTypeName,
            lifetime,
            exposedServiceTypeNames,
            replace: false,
            tryAdd: false,
            key: null);
    }
}
