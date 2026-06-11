namespace AtomUI.City.Security;

public sealed class InMemoryCommandAuthorizationDescriptorProvider : ICommandAuthorizationDescriptorProvider
{
    private readonly Dictionary<string, CommandAuthorizationDescriptor> _descriptors = new(StringComparer.Ordinal);
    private readonly object _syncRoot = new();
    private long _revision;

    public event EventHandler<CommandAuthorizationChangedEventArgs>? DescriptorChanged;

    public long Revision
    {
        get
        {
            lock (_syncRoot)
            {
                return _revision;
            }
        }
    }

    public bool Add(CommandAuthorizationDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        long revision;

        lock (_syncRoot)
        {
            if (_descriptors.ContainsKey(descriptor.CommandId))
            {
                return false;
            }

            _descriptors.Add(descriptor.CommandId, descriptor);
            revision = ++_revision;
        }

        DescriptorChanged?.Invoke(
            this,
            new CommandAuthorizationChangedEventArgs(
                CommandAuthorizationChangeReason.DescriptorChanged,
                revision,
                descriptor.CommandId));

        return true;
    }

    public bool Remove(string commandId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);
        long revision;

        lock (_syncRoot)
        {
            if (!_descriptors.Remove(commandId))
            {
                return false;
            }

            revision = ++_revision;
        }

        DescriptorChanged?.Invoke(
            this,
            new CommandAuthorizationChangedEventArgs(
                CommandAuthorizationChangeReason.DescriptorChanged,
                revision,
                commandId));

        return true;
    }

    public ValueTask<CommandAuthorizationDescriptor?> GetDescriptorAsync(
        CommandAuthorizationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult<CommandAuthorizationDescriptor?>(null);
        }

        lock (_syncRoot)
        {
            _descriptors.TryGetValue(context.CommandId, out var descriptor);

            return ValueTask.FromResult<CommandAuthorizationDescriptor?>(descriptor);
        }
    }
}
