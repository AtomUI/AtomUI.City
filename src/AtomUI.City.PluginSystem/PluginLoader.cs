using System.Reflection;
using System.Runtime.Loader;

namespace AtomUI.City.PluginSystem;

public sealed class PluginLoader
{
    public ValueTask<PluginLoadResult> LoadAsync(
        PluginDescriptor descriptor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(descriptor.MainAssemblyPath))
        {
            return ValueTask.FromResult(
                PluginLoadResult.Failed(
                    [
                        new PluginDiagnostic(
                            PluginDiagnosticIds.MainAssemblyNotFound,
                            $"Plugin main assembly '{descriptor.Manifest.MainAssembly}' was not found.",
                            descriptor.PluginId,
                            "mainAssembly",
                            descriptor.MainAssemblyPath),
                    ]));
        }

        var loadContext = new PluginAssemblyLoadContext(descriptor);

        try
        {
            var assembly = loadContext.LoadFromAssemblyPath(descriptor.MainAssemblyPath);
            return ValueTask.FromResult(PluginLoadResult.Success(new PluginRuntime(
                descriptor,
                assembly,
                loadContext)));
        }
        catch (Exception exception)
        {
            loadContext.Unload();

            return ValueTask.FromResult(
                PluginLoadResult.Failed(
                    [
                        new PluginDiagnostic(
                            PluginDiagnosticIds.MainAssemblyNotFound,
                            $"Plugin main assembly '{descriptor.Manifest.MainAssembly}' failed to load: {exception.Message}",
                            descriptor.PluginId,
                            "mainAssembly",
                            descriptor.MainAssemblyPath),
                    ]));
        }
    }

    private sealed class PluginAssemblyLoadContext : AssemblyLoadContext
    {
        public PluginAssemblyLoadContext(PluginDescriptor descriptor)
            : base($"AtomUI.City.Plugin:{descriptor.PluginId}:{descriptor.Version}", isCollectible: descriptor.Manifest.Unloadable)
        {
            Descriptor = descriptor;
        }

        public PluginDescriptor Descriptor { get; }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            var privateAssemblyPath = Path.Combine(
                Descriptor.RootPath,
                "lib",
                Descriptor.Manifest.TargetFramework,
                $"{assemblyName.Name}.dll");

            return File.Exists(privateAssemblyPath)
                ? LoadFromAssemblyPath(privateAssemblyPath)
                : null;
        }
    }
}
