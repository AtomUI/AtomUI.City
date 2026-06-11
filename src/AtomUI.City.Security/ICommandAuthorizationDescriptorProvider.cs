namespace AtomUI.City.Security;

public interface ICommandAuthorizationDescriptorProvider
{
    event EventHandler<CommandAuthorizationChangedEventArgs>? DescriptorChanged;

    ValueTask<CommandAuthorizationDescriptor?> GetDescriptorAsync(
        CommandAuthorizationContext context,
        CancellationToken cancellationToken = default);
}
