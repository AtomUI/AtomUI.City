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

    [Fact]
    public void BuildReturnsReadonlyServiceRegistrationCollections()
    {
        var result = ServiceRegistrationManifestBuilder.Build(
            [
                Registration("Sample.App.SystemClock", ServiceRegistrationLifetime.Singleton, "Sample.App.IClock"),
            ]);
        var registrations = Assert.IsAssignableFrom<IList<ServiceRegistrationMetadata>>(result.Manifest.Registrations);
        var diagnostics = Assert.IsAssignableFrom<IList<GeneratorDiagnostic>>(result.Diagnostics);

        Assert.Throws<NotSupportedException>(() => registrations[0] = Registration("Sample.App.ChangedClock", ServiceRegistrationLifetime.Singleton, "Sample.App.IClock"));
        Assert.Throws<NotSupportedException>(() => diagnostics.Add(new GeneratorDiagnostic(GeneratorDiagnostics.InvalidManifestInput, "Changed")));
        Assert.Equal("Sample.App.SystemClock", result.Manifest.Registrations[0].ImplementationTypeName);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void ServiceRegistrationMetadataExposedServicesRejectExternalMutation()
    {
        var exposedServices = new List<string> { "Sample.App.IClock" };
        var registration = new ServiceRegistrationMetadata(
            "Sample.App.SystemClock",
            ServiceRegistrationLifetime.Singleton,
            exposedServices,
            replace: false,
            tryAdd: false,
            key: null);
        var exposed = Assert.IsAssignableFrom<IList<string>>(registration.ExposedServiceTypeNames);

        Assert.Throws<NotSupportedException>(() => exposed[0] = "Sample.App.IChangedClock");
        Assert.Equal("Sample.App.IClock", registration.ExposedServiceTypeNames[0]);
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
