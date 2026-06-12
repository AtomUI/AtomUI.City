namespace AtomUI.City.Presentation;

public interface IPresentationPluginUnloadCoordinator
{
    ValueTask<PresentationPluginUnloadResult> CleanupAsync(
        PresentationPluginUnloadRequest request,
        CancellationToken cancellationToken = default);
}
