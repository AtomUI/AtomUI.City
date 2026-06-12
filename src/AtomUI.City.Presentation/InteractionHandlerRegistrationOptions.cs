using AtomUI.City.Mvvm;

namespace AtomUI.City.Presentation;

public sealed class InteractionHandlerRegistrationOptions
{
    public IActivationScope? ActivationScope { get; init; }

    public string? PluginId { get; init; }

    public string? ContributionId { get; init; }
}
