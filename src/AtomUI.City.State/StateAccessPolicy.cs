namespace AtomUI.City.State;

public enum StateAccessPolicy
{
    ReadOnly,
    OwnerWrite,
    HostWrite,
    AuthorizedWrite,
    PluginIsolated,
}
