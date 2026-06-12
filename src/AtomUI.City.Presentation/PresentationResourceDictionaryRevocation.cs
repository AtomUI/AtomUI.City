namespace AtomUI.City.Presentation;

public sealed class PresentationResourceDictionaryRevocation
{
    public PresentationResourceDictionaryRevocation(
        string pluginId,
        string? contributionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        PluginId = pluginId;
        ContributionId = string.IsNullOrWhiteSpace(contributionId) ? null : contributionId;
    }

    public string PluginId { get; }

    public string? ContributionId { get; }
}
