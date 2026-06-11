using AtomUI.City.Hosting;

namespace AtomUI.City.Modularity;

internal static class ModuleRegistrationStore
{
    private const string Key = "AtomUI.City.Modularity.ModuleRegistrations";

    public static void Add(IApplicationHostBuilder builder, ModuleRegistration registration)
    {
        var registrations = GetOrCreateRegistrations(builder);

        if (registrations.Any(existing => existing.ModuleType == registration.ModuleType))
        {
            return;
        }

        registrations.Add(registration);
    }

    public static IReadOnlyList<ModuleRegistration> GetRegistrations(IApplicationHostBuilder builder)
    {
        return GetOrCreateRegistrations(builder).ToArray();
    }

    private static List<ModuleRegistration> GetOrCreateRegistrations(IApplicationHostBuilder builder)
    {
        if (builder.Properties.TryGetValue(Key, out var value) &&
            value is List<ModuleRegistration> registrations)
        {
            return registrations;
        }

        registrations = [];
        builder.Properties[Key] = registrations;

        return registrations;
    }
}
