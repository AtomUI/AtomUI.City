namespace AtomUI.City.Generators.DependencyInjection;

public sealed class ServiceRegistrationManifest
{
    public ServiceRegistrationManifest(IReadOnlyList<ServiceRegistrationMetadata> registrations)
    {
        Registrations = registrations ?? throw new ArgumentNullException(nameof(registrations));
    }

    public IReadOnlyList<ServiceRegistrationMetadata> Registrations { get; }
}
