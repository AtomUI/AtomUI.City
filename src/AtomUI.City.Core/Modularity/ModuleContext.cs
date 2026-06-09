using AtomUI.City.Hosting;

namespace AtomUI.City.Modularity;

public sealed class ModuleContext
{
    public ModuleContext(string name, ApplicationContext applicationContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(applicationContext);

        Name = name;
        ApplicationContext = applicationContext;
    }

    public string Name { get; }

    public ApplicationContext ApplicationContext { get; }
}
