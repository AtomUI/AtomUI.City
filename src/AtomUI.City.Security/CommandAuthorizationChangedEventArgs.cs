namespace AtomUI.City.Security;

public sealed class CommandAuthorizationChangedEventArgs : EventArgs
{
    public CommandAuthorizationChangedEventArgs(
        CommandAuthorizationChangeReason reason,
        long revision,
        string? commandId = null)
    {
        Reason = reason;
        Revision = revision;
        CommandId = commandId;
    }

    public CommandAuthorizationChangeReason Reason { get; }

    public long Revision { get; }

    public string? CommandId { get; }
}
