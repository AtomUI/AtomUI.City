namespace AtomUI.City.Security;

public sealed class CommandAuthorizationState
{
    public CommandAuthorizationState(
        string commandId,
        bool canExecute,
        bool isVisible,
        CommandUnauthorizedBehavior unauthorizedBehavior,
        AuthorizationResult authorization,
        long revision,
        string? deniedMessageKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        ArgumentNullException.ThrowIfNull(authorization);

        CommandId = commandId;
        CanExecute = canExecute;
        IsVisible = isVisible;
        UnauthorizedBehavior = unauthorizedBehavior;
        Authorization = authorization;
        Revision = revision;
        DeniedMessageKey = deniedMessageKey;
    }

    public string CommandId { get; }

    public bool CanExecute { get; }

    public bool IsVisible { get; }

    public CommandUnauthorizedBehavior UnauthorizedBehavior { get; }

    public AuthorizationResult Authorization { get; }

    public long Revision { get; }

    public string? DeniedMessageKey { get; }
}
