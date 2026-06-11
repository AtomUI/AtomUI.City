namespace AtomUI.City.Security;

public sealed class CommandAuthorizationContext
{
    public CommandAuthorizationContext(
        string commandId,
        string? resourceName = null,
        string? contributionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        CommandId = commandId;
        ResourceName = resourceName;
        ContributionId = contributionId;
    }

    public string CommandId { get; }

    public string? ResourceName { get; }

    public string? ContributionId { get; }
}
