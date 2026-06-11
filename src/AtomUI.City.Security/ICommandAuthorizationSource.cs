namespace AtomUI.City.Security;

public interface ICommandAuthorizationSource
{
    event EventHandler<CommandAuthorizationChangedEventArgs>? AuthorizationChanged;

    ValueTask<CommandAuthorizationState> GetStateAsync(
        CommandAuthorizationContext context,
        CancellationToken cancellationToken = default);

    ValueTask<AuthorizationResult> CheckExecutionAsync(
        CommandAuthorizationContext context,
        CancellationToken cancellationToken = default);
}
