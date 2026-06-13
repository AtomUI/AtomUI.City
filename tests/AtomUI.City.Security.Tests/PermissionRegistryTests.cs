using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class PermissionRegistryTests
{
    [Fact]
    public void AddStoresPermissionAndIncrementsRevision()
    {
        var registry = new PermissionRegistry();

        var added = registry.Add(new PermissionDescriptor(
            "settings.read",
            displayNameKey: "Permissions.Settings.Read",
            category: "Settings",
            contributionId: "Host"));

        Assert.True(added);
        Assert.Equal(1, registry.Revision);
        Assert.True(registry.TryGet("settings.read", out var descriptor));
        Assert.Equal("Permissions.Settings.Read", descriptor.DisplayNameKey);
        Assert.Equal("Settings", descriptor.Category);
        Assert.Equal("Host", descriptor.ContributionId);
    }

    [Fact]
    public void AddRejectsDuplicatePermissionWithoutChangingRevision()
    {
        var registry = new PermissionRegistry();
        registry.Add(new PermissionDescriptor("settings.read"));

        var added = registry.Add(new PermissionDescriptor("settings.read"));

        Assert.False(added);
        Assert.Equal(1, registry.Revision);
    }

    [Fact]
    public void RemoveByContributionRevokesMatchingPermissions()
    {
        var registry = new PermissionRegistry();
        registry.Add(new PermissionDescriptor("plugin.sales.export", contributionId: "SalesPlugin"));
        registry.Add(new PermissionDescriptor("settings.read", contributionId: "Host"));

        var removed = registry.RemoveByContribution("SalesPlugin");

        Assert.Equal(1, removed);
        Assert.Equal(3, registry.Revision);
        Assert.False(registry.Contains("plugin.sales.export"));
        Assert.True(registry.Contains("settings.read"));
    }

    [Fact]
    public void PermissionsRejectsExternalListMutation()
    {
        var registry = new PermissionRegistry();
        registry.Add(new PermissionDescriptor("settings.read"));

        var permissions = registry.Permissions;

        var mutable = Assert.IsAssignableFrom<IList<PermissionDescriptor>>(permissions);
        Assert.Throws<NotSupportedException>(() => mutable[0] = new PermissionDescriptor("settings.write"));
    }
}
