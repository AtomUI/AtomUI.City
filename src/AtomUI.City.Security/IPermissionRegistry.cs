using System.Diagnostics.CodeAnalysis;

namespace AtomUI.City.Security;

public interface IPermissionRegistry
{
    long Revision { get; }

    IReadOnlyCollection<PermissionDescriptor> Permissions { get; }

    bool Contains(string name);

    bool TryGet(
        string name,
        [NotNullWhen(true)] out PermissionDescriptor? descriptor);
}
