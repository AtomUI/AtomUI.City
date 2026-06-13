namespace AtomUI.City.Generators.DependencyInjection;

public sealed class ServiceRegistrationManifest
{
    public ServiceRegistrationManifest(IReadOnlyList<ServiceRegistrationMetadata> registrations)
    {
        Registrations = Array.AsReadOnly((registrations ?? throw new ArgumentNullException(nameof(registrations))).ToArray());
    }

    public IReadOnlyList<ServiceRegistrationMetadata> Registrations { get; }
}
