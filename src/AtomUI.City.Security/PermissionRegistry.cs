using System.Diagnostics.CodeAnalysis;

namespace AtomUI.City.Security;

public sealed class PermissionRegistry : IPermissionRegistry
{
    private readonly Dictionary<string, PermissionDescriptor> _permissions = new(StringComparer.Ordinal);
    private readonly object _syncRoot = new();
    private long _revision;

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
                return _permissions.Values.ToArray();
            }
        }
    }

    public bool Add(PermissionDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        lock (_syncRoot)
        {
            if (_permissions.ContainsKey(descriptor.Name))
            {
                return false;
            }

            _permissions.Add(descriptor.Name, descriptor);
            _revision++;

            return true;
        }
    }

    public bool Remove(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_syncRoot)
        {
            if (!_permissions.Remove(name))
            {
                return false;
            }

            _revision++;

            return true;
        }
    }

    public int RemoveByContribution(string contributionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contributionId);

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
                _revision++;
            }

            return names.Length;
        }
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
