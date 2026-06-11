namespace AtomUI.City.Security;

public sealed class CommandAuthorizationDescriptor
{
    public CommandAuthorizationDescriptor(
        string commandId,
        AuthorizationPolicy policy,
        CommandUnauthorizedBehavior unauthorizedBehavior = CommandUnauthorizedBehavior.Disable,
        string? deniedMessageKey = null,
        string? contributionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        ArgumentNullException.ThrowIfNull(policy);

        CommandId = commandId;
        Policy = policy;
        UnauthorizedBehavior = unauthorizedBehavior;
        DeniedMessageKey = deniedMessageKey;
        ContributionId = contributionId;
    }

    public string CommandId { get; }

    public AuthorizationPolicy Policy { get; }

    public CommandUnauthorizedBehavior UnauthorizedBehavior { get; }

    public string? DeniedMessageKey { get; }

    public string? ContributionId { get; }
}
