namespace AtomUI.City.Modularity;

public interface IModule
{
    ValueTask PreConfigureAsync(ModuleContext context);

    ValueTask ConfigureAsync(ModuleContext context);

    ValueTask InitializeAsync(ModuleContext context);

    ValueTask ShutdownAsync(ModuleContext context);
}
