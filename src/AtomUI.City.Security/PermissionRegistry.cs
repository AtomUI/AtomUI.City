using System.Diagnostics.CodeAnalysis;

namespace AtomUI.City.Security;

public sealed class PermissionRegistry : IPermissionRegistry
{
    private readonly Dictionary<string, PermissionDescriptor> _permissions = new(StringComparer.Ordinal);
    private readonly object _syncRoot = new();
    private long _revision;

    public event EventHandler<PermissionRegistryChangedEventArgs>? Changed;

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

    public IReadOnlyCollection<PermissionDescriptor> Permissions
    {
        get
        {
            lock (_syncRoot)
            {
                return Array.AsReadOnly(_permissions.Values.ToArray());
            }
        }
    }

    public bool Add(PermissionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        long revision;

        lock (_syncRoot)
        {
            if (_permissions.ContainsKey(descriptor.Name))
            {
                return false;
            }

            _permissions.Add(descriptor.Name, descriptor);
            revision = ++_revision;
        }

        Changed?.Invoke(
            this,
            new PermissionRegistryChangedEventArgs(
                revision,
                descriptor.Name,
                descriptor.ContributionId));

        return true;
    }

    public bool Remove(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        long revision;
        PermissionDescriptor removed;

        lock (_syncRoot)
        {
            if (!_permissions.Remove(name, out var descriptor))
            {
                return false;
            }

            removed = descriptor;
            revision = ++_revision;
        }

        Changed?.Invoke(
            this,
            new PermissionRegistryChangedEventArgs(
                revision,
                removed.Name,
                removed.ContributionId));

        return true;
    }

    public int RemoveByContribution(string contributionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contributionId);
        long revision;
        int removedCount;

        lock (_syncRoot)
        {
            var names = _permissions
                .Where(pair => string.Equals(pair.Value.ContributionId, contributionId, StringComparison.Ordinal))
                .Select(pair => pair.Key)
                .ToArray();

            foreach (var name in names)
            {
                _permissions.Remove(name);
            }

            if (names.Length > 0)
            {
                revision = ++_revision;
            }
            else
            {
                return 0;
            }

            removedCount = names.Length;
        }

        Changed?.Invoke(
            this,
            new PermissionRegistryChangedEventArgs(
                revision,
                permissionName: null,
                contributionId));

        return removedCount;
    }

    public bool Contains(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_syncRoot)
        {
            return _permissions.ContainsKey(name);
        }
    }

    public bool TryGet(
        string name,
        [NotNullWhen(true)] out PermissionDescriptor? descriptor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_syncRoot)
        {
            return _permissions.TryGetValue(name, out descriptor);
        }
    }
}
