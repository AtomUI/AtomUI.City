using System.Reflection;
using System.Runtime.Loader;

namespace AtomUI.City.PluginSystem;

public sealed class PluginRuntime
{
    private Assembly? _mainAssembly;
    private AssemblyLoadContext? _loadContext;

    internal PluginRuntime(
        PluginDescriptor descriptor,
        Assembly mainAssembly,
        AssemblyLoadContext loadContext)
    {
        Descriptor = descriptor;
        _mainAssembly = mainAssembly;
        _loadContext = loadContext;
        State = PluginRuntimeState.Loaded;
    }

    public PluginDescriptor Descriptor { get; }

    public PluginRuntimeState State { get; private set; }

    public Assembly MainAssembly => _mainAssembly ??
        throw new InvalidOperationException("Plugin main assembly is not available after unload.");

    public void Activate()
    {
        if (State is not (PluginRuntimeState.Loaded or PluginRuntimeState.Inactive))
        {
            throw new InvalidOperationException($"Plugin cannot be activated from state '{State}'.");
        }

        State = PluginRuntimeState.Active;
    }

    public ValueTask DeactivateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (State == PluginRuntimeState.Active)
        {
            State = PluginRuntimeState.Deactivating;
            State = PluginRuntimeState.Inactive;
        }

        return ValueTask.CompletedTask;
    }

    public async ValueTask UnloadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (State == PluginRuntimeState.Active)
        {
            await DeactivateAsync(cancellationToken).ConfigureAwait(false);
        }

        if (State == PluginRuntimeState.Unloaded)
        {
            return;
        }

        State = PluginRuntimeState.Unloading;
        _mainAssembly = null;
        var loadContext = _loadContext;
        _loadContext = null;
        loadContext?.Unload();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        State = PluginRuntimeState.Unloaded;
    }
}
