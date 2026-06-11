using AtomUI.City.Generators.Diagnostics;

namespace AtomUI.City.Generators.DependencyInjection;

public static class ServiceRegistrationManifestBuilder
{
    public static ServiceRegistrationManifestResult Build(IReadOnlyList<ServiceRegistrationMetadata> registrations)
    {
        if (registrations is null)
        {
            throw new ArgumentNullException(nameof(registrations));
        }

        var diagnostics = new List<GeneratorDiagnostic>();
        var exposedServices = new Dictionary<string, ServiceRegistrationMetadata>(StringComparer.Ordinal);

        foreach (var registration in registrations)
        {
            foreach (var exposedServiceTypeName in registration.ExposedServiceTypeNames)
            {
                if (!exposedServices.TryGetValue(exposedServiceTypeName, out var existingRegistration))
                {
                    exposedServices.Add(exposedServiceTypeName, registration);
                    continue;
                }

                if (registration.Replace || registration.TryAdd || existingRegistration.Replace || existingRegistration.TryAdd)
                {
                    continue;
                }

                diagnostics.Add(new GeneratorDiagnostic(
                    GeneratorDiagnostics.InvalidManifestInput,
                    $"Service '{exposedServiceTypeName}' is exposed by multiple registrations.",
                    exposedServiceTypeName));
            }
        }

        if (diagnostics.Count > 0)
        {
            return new ServiceRegistrationManifestResult(new ServiceRegistrationManifest([]), diagnostics);
        }

        var manifest = new ServiceRegistrationManifest(
            registrations
                .OrderBy(registration => registration.ImplementationTypeName, StringComparer.Ordinal)
                .ThenBy(registration => registration.Lifetime)
                .ToArray());

        return new ServiceRegistrationManifestResult(manifest, diagnostics);
    }
}
