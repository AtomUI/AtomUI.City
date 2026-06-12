using AtomUI.City.Lifecycle;

namespace AtomUI.City.Presentation;

public interface IPresentationRuntime
{
    PresentationRuntimeState State { get; }

    bool IsReady { get; }

    LifecycleScope? PresentationScope { get; }

    ValueTask StartAsync(
        LifecycleScope applicationScope,
        string presentationScopeId = "presentation",
        CancellationToken cancellationToken = default);

    LifecycleScope CreateWindowScope(string windowScopeId);

    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
