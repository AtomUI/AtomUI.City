namespace AtomUI.City.Presentation;

public sealed class PresentationPluginUnloadResult
{
    public PresentationPluginUnloadResult(
        string pluginId,
        string? contributionId,
        int closedViewCount,
        int revokedInteractionHandlerCount,
        int revokedResourceContributionCount,
        bool resourceDictionariesRevoked,
        IReadOnlyList<PresentationPluginUnloadError> errors)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        ArgumentNullException.ThrowIfNull(errors);

        PluginId = pluginId;
        ContributionId = string.IsNullOrWhiteSpace(contributionId) ? null : contributionId;
        ClosedViewCount = closedViewCount;
        RevokedInteractionHandlerCount = revokedInteractionHandlerCount;
        RevokedResourceContributionCount = revokedResourceContributionCount;
        ResourceDictionariesRevoked = resourceDictionariesRevoked;
        Errors = errors.ToArray();
    }

    public string PluginId { get; }

    public string? ContributionId { get; }

    public int ClosedViewCount { get; }

    public int RevokedInteractionHandlerCount { get; }

    public int RevokedResourceContributionCount { get; }

    public bool ResourceDictionariesRevoked { get; }

    public IReadOnlyList<PresentationPluginUnloadError> Errors { get; }

    public bool Succeeded => Errors.Count == 0;
}
