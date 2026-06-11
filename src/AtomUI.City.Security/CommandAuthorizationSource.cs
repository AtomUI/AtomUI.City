namespace AtomUI.City.Security;

public sealed class CommandAuthorizationSource : ICommandAuthorizationSource, IDisposable
{
    private readonly IAuthorizationEvaluator _authorizationEvaluator;
    private readonly ICurrentPrincipalAccessor _principalAccessor;
    private readonly ICommandAuthorizationDescriptorProvider _descriptorProvider;
    private readonly IAuthenticationStateProvider? _authenticationStateProvider;
    private readonly IPermissionRegistry? _permissionRegistry;
    private long _revision;
    private bool _disposed;

    public CommandAuthorizationSource(
        IAuthorizationEvaluator authorizationEvaluator,
        ICurrentPrincipalAccessor principalAccessor,
        ICommandAuthorizationDescriptorProvider descriptorProvider,
        IAuthenticationStateProvider? authenticationStateProvider = null,
        IPermissionRegistry? permissionRegistry = null)
    {
        ArgumentNullException.ThrowIfNull(authorizationEvaluator);
        ArgumentNullException.ThrowIfNull(principalAccessor);
        ArgumentNullException.ThrowIfNull(descriptorProvider);

        _authorizationEvaluator = authorizationEvaluator;
        _principalAccessor = principalAccessor;
        _descriptorProvider = descriptorProvider;
        _authenticationStateProvider = authenticationStateProvider ?? principalAccessor as IAuthenticationStateProvider;
        _permissionRegistry = permissionRegistry;

        if (_authenticationStateProvider is not null)
        {
            _authenticationStateProvider.StateChanged += OnAuthenticationStateChanged;
        }

        _descriptorProvider.DescriptorChanged += OnDescriptorChanged;

        if (_permissionRegistry is not null)
        {
            _permissionRegistry.Changed += OnPermissionRegistryChanged;
        }
    }

    public event EventHandler<CommandAuthorizationChangedEventArgs>? AuthorizationChanged;

    public async ValueTask<CommandAuthorizationState> GetStateAsync(
        CommandAuthorizationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (cancellationToken.IsCancellationRequested)
        {
            return CreateCancelledState(context);
        }

        var descriptor = await _descriptorProvider.GetDescriptorAsync(context, cancellationToken)
            .ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
        {
            return CreateCancelledState(context);
        }

        if (descriptor is null)
        {
            return new CommandAuthorizationState(
                context.CommandId,
                canExecute: true,
                isVisible: true,
                CommandUnauthorizedBehavior.Disable,
                AuthorizationResult.Allowed(),
                _revision);
        }

        var result = await EvaluateDescriptorAsync(context, descriptor, cancellationToken)
            .ConfigureAwait(false);
        var canExecute = result.Succeeded;
        var isVisible = result.Succeeded || descriptor.UnauthorizedBehavior != CommandUnauthorizedBehavior.Hide;

        return new CommandAuthorizationState(
            context.CommandId,
            canExecute,
            isVisible,
            descriptor.UnauthorizedBehavior,
            result,
            _revision,
            descriptor.DeniedMessageKey);
    }

    public async ValueTask<AuthorizationResult> CheckExecutionAsync(
        CommandAuthorizationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (cancellationToken.IsCancellationRequested)
        {
            return AuthorizationResult.Cancelled();
        }

        var descriptor = await _descriptorProvider.GetDescriptorAsync(context, cancellationToken)
            .ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested)
        {
            return AuthorizationResult.Cancelled();
        }

        return descriptor is null
            ? AuthorizationResult.Allowed()
            : await EvaluateDescriptorAsync(context, descriptor, cancellationToken)
                .ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_authenticationStateProvider is not null)
        {
            _authenticationStateProvider.StateChanged -= OnAuthenticationStateChanged;
        }

        _descriptorProvider.DescriptorChanged -= OnDescriptorChanged;

        if (_permissionRegistry is not null)
        {
            _permissionRegistry.Changed -= OnPermissionRegistryChanged;
        }

        _disposed = true;
    }

    private ValueTask<AuthorizationResult> EvaluateDescriptorAsync(
        CommandAuthorizationContext context,
        CommandAuthorizationDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        return _authorizationEvaluator.EvaluateAsync(
            new AuthorizationRequest(
                _principalAccessor.Principal,
                descriptor.Policy,
                context.ResourceName ?? context.CommandId,
                context.ContributionId ?? descriptor.ContributionId),
            cancellationToken);
    }

    private CommandAuthorizationState CreateCancelledState(CommandAuthorizationContext context)
    {
        return new CommandAuthorizationState(
            context.CommandId,
            canExecute: false,
            isVisible: true,
            CommandUnauthorizedBehavior.Disable,
            AuthorizationResult.Cancelled(),
            _revision);
    }

    private void OnAuthenticationStateChanged(
        object? sender,
        AuthenticationStateChangedEventArgs args)
    {
        var revision = Interlocked.Increment(ref _revision);
        AuthorizationChanged?.Invoke(
            this,
            new CommandAuthorizationChangedEventArgs(
                CommandAuthorizationChangeReason.AuthenticationStateChanged,
                revision));
    }

    private void OnDescriptorChanged(
        object? sender,
        CommandAuthorizationChangedEventArgs args)
    {
        var revision = Interlocked.Increment(ref _revision);
        AuthorizationChanged?.Invoke(
            this,
            new CommandAuthorizationChangedEventArgs(
                CommandAuthorizationChangeReason.DescriptorChanged,
                revision,
                args.CommandId));
    }

    private void OnPermissionRegistryChanged(
        object? sender,
        PermissionRegistryChangedEventArgs args)
    {
        var revision = Interlocked.Increment(ref _revision);
        AuthorizationChanged?.Invoke(
            this,
            new CommandAuthorizationChangedEventArgs(
                CommandAuthorizationChangeReason.PermissionChanged,
                revision));
    }
}
