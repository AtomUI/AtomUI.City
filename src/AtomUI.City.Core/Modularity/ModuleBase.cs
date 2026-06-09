namespace AtomUI.City.Modularity;

public abstract class ModuleBase : IModule
{
    public virtual ValueTask PreConfigureAsync(ModuleContext context) => ValueTask.CompletedTask;

    public virtual ValueTask ConfigureAsync(ModuleContext context) => ValueTask.CompletedTask;

    public virtual ValueTask InitializeAsync(ModuleContext context) => ValueTask.CompletedTask;

    public virtual ValueTask ShutdownAsync(ModuleContext context) => ValueTask.CompletedTask;
}
