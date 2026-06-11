using System.Security.Claims;
using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class CommandAuthorizationSourceTests
{
    [Fact]
    public async Task GetStateAsyncAllowsCommandWithoutAuthorizationDescriptor()
    {
        var source = CreateSource();

        var state = await source.GetStateAsync(new CommandAuthorizationContext("settings.open"));

        Assert.True(state.CanExecute);
        Assert.True(state.IsVisible);
        Assert.Equal(AuthorizationResultStatus.Allowed, state.Authorization.Status);
    }

    [Fact]
    public async Task GetStateAsyncDisablesProtectedCommandForAnonymousPrincipal()
    {
        var source = CreateSource(provider =>
        {
            provider.Add(new CommandAuthorizationDescriptor(
                "settings.open",
                AuthorizationPolicy.RequireAuthenticated("SignedIn")));
        });

        var state = await source.GetStateAsync(new CommandAuthorizationContext("settings.open"));

        Assert.False(state.CanExecute);
        Assert.True(state.IsVisible);
        Assert.Equal(CommandUnauthorizedBehavior.Disable, state.UnauthorizedBehavior);
        Assert.Equal(AuthorizationResultStatus.Challenge, state.Authorization.Status);
    }

    [Fact]
    public async Task GetStateAsyncHidesForbiddenCommandWhenDescriptorRequestsHidden()
    {
        var store = new AuthenticationStateStore();
        store.SetAuthenticated(CreatePrincipal(permissions: []));
        var permissions = new PermissionRegistry();
        permissions.Add(new PermissionDescriptor("settings.write"));
        var source = CreateSource(
            provider =>
            {
                provider.Add(new CommandAuthorizationDescriptor(
                    "settings.save",
                    AuthorizationPolicy.RequirePermission("CanWriteSettings", "settings.write"),
                    CommandUnauthorizedBehavior.Hide,
                    deniedMessageKey: "Permissions.Settings.WriteDenied"));
            },
            store,
            permissions);

        var state = await source.GetStateAsync(new CommandAuthorizationContext("settings.save"));

        Assert.False(state.CanExecute);
        Assert.False(state.IsVisible);
        Assert.Equal("Permissions.Settings.WriteDenied", state.DeniedMessageKey);
        Assert.Equal(AuthorizationResultStatus.Forbidden, state.Authorization.Status);
    }

    [Fact]
    public async Task GetStateAsyncAllowsCommandWhenPermissionIsGranted()
    {
        var store = new AuthenticationStateStore();
        store.SetAuthenticated(CreatePrincipal(permissions: ["settings.write"]));
        var permissions = new PermissionRegistry();
        permissions.Add(new PermissionDescriptor("settings.write"));
        var source = CreateSource(
            provider =>
            {
                provider.Add(new CommandAuthorizationDescriptor(
                    "settings.save",
                    AuthorizationPolicy.RequirePermission("CanWriteSettings", "settings.write")));
            },
            store,
            permissions);

        var state = await source.GetStateAsync(new CommandAuthorizationContext("settings.save"));

        Assert.True(state.CanExecute);
        Assert.True(state.IsVisible);
        Assert.Equal(AuthorizationResultStatus.Allowed, state.Authorization.Status);
    }

    [Fact]
    public void AuthenticationStateChangeRaisesAuthorizationChanged()
    {
        var store = new AuthenticationStateStore();
        var source = CreateSource(authenticationStateProvider: store);
        CommandAuthorizationChangedEventArgs? observed = null;
        source.AuthorizationChanged += (_, args) => observed = args;

        store.SetAuthenticated(CreatePrincipal(permissions: []));

        Assert.NotNull(observed);
        Assert.Equal(CommandAuthorizationChangeReason.AuthenticationStateChanged, observed.Reason);
        Assert.Equal(1, observed.Revision);
    }

    [Fact]
    public void PermissionRegistryChangeRaisesAuthorizationChanged()
    {
        var permissions = new PermissionRegistry();
        var source = CreateSource(permissions: permissions);
        CommandAuthorizationChangedEventArgs? observed = null;
        source.AuthorizationChanged += (_, args) => observed = args;

        permissions.Add(new PermissionDescriptor("settings.write"));

        Assert.NotNull(observed);
        Assert.Equal(CommandAuthorizationChangeReason.PermissionChanged, observed.Reason);
        Assert.Equal(1, observed.Revision);
    }

    [Fact]
    public void DescriptorChangeRaisesAuthorizationChangedForCommand()
    {
        var provider = new InMemoryCommandAuthorizationDescriptorProvider();
        var source = CreateSource(provider: provider);
        CommandAuthorizationChangedEventArgs? observed = null;
        source.AuthorizationChanged += (_, args) => observed = args;

        provider.Add(new CommandAuthorizationDescriptor(
            "settings.save",
            AuthorizationPolicy.RequireAuthenticated("SignedIn")));

        Assert.NotNull(observed);
        Assert.Equal(CommandAuthorizationChangeReason.DescriptorChanged, observed.Reason);
        Assert.Equal("settings.save", observed.CommandId);
    }

    [Fact]
    public async Task CheckExecutionAsyncReevaluatesAuthorizationBeforeExecution()
    {
        var store = new AuthenticationStateStore();
        store.SetAuthenticated(CreatePrincipal(permissions: ["settings.write"]));
        var permissions = new PermissionRegistry();
        permissions.Add(new PermissionDescriptor("settings.write"));
        var source = CreateSource(
            provider =>
            {
                provider.Add(new CommandAuthorizationDescriptor(
                    "settings.save",
                    AuthorizationPolicy.RequirePermission("CanWriteSettings", "settings.write")));
            },
            store,
            permissions);

        permissions.Remove("settings.write");

        var result = await source.CheckExecutionAsync(new CommandAuthorizationContext("settings.save"));

        Assert.False(result.Succeeded);
        Assert.Equal(AuthorizationResultStatus.Failed, result.Status);
        Assert.Equal(SecurityFailureKind.PermissionNotFound, result.FailureKind);
    }

    [Fact]
    public async Task GetStateAsyncDoesNotAllowCommandWhenAuthorizationIsCancelled()
    {
        var source = CreateSource(provider =>
        {
            provider.Add(new CommandAuthorizationDescriptor(
                "settings.save",
                AuthorizationPolicy.RequireAuthenticated("SignedIn")));
        });
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var state = await source.GetStateAsync(
            new CommandAuthorizationContext("settings.save"),
            cancellation.Token);

        Assert.False(state.CanExecute);
        Assert.Equal(AuthorizationResultStatus.Cancelled, state.Authorization.Status);
    }

    [Fact]
    public async Task CheckExecutionAsyncReturnsCancelledWhenAuthorizationIsCancelled()
    {
        var source = CreateSource(provider =>
        {
            provider.Add(new CommandAuthorizationDescriptor(
                "settings.save",
                AuthorizationPolicy.RequireAuthenticated("SignedIn")));
        });
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await source.CheckExecutionAsync(
            new CommandAuthorizationContext("settings.save"),
            cancellation.Token);

        Assert.Equal(AuthorizationResultStatus.Cancelled, result.Status);
    }

    private static CommandAuthorizationSource CreateSource(
        Action<InMemoryCommandAuthorizationDescriptorProvider>? configureProvider = null,
        AuthenticationStateStore? authenticationStateProvider = null,
        PermissionRegistry? permissions = null,
        InMemoryCommandAuthorizationDescriptorProvider? provider = null)
    {
        provider ??= new InMemoryCommandAuthorizationDescriptorProvider();
        configureProvider?.Invoke(provider);
        authenticationStateProvider ??= new AuthenticationStateStore();
        permissions ??= new PermissionRegistry();

        return new CommandAuthorizationSource(
            new AuthorizationEvaluator(permissions),
            authenticationStateProvider,
            provider,
            permissionRegistry: permissions);
    }

    private static ClaimsPrincipal CreatePrincipal(IReadOnlyCollection<string> permissions)
    {
        var identity = new ClaimsIdentity(authenticationType: "Test");

        foreach (var permission in permissions)
        {
            identity.AddClaim(new Claim("permission", permission));
        }

        return new ClaimsPrincipal(identity);
    }
}
