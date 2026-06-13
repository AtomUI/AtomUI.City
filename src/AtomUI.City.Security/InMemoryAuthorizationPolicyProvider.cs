using System.Diagnostics.CodeAnalysis;

namespace AtomUI.City.Security;

public sealed class InMemoryAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly Dictionary<string, AuthorizationPolicy> _policies = new(StringComparer.Ordinal);
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

    public IReadOnlyCollection<AuthorizationPolicy> Policies
    {
        get
        {
            lock (_syncRoot)
            {
                return Array.AsReadOnly(_policies.Values.ToArray());
            }
        }
    }

    public bool Add(AuthorizationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        lock (_syncRoot)
        {
            if (_policies.ContainsKey(policy.Name))
            {
                return false;
            }

            _policies.Add(policy.Name, policy);
            _revision++;

            return true;
        }
    }

    public bool Remove(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_syncRoot)
        {
            if (!_policies.Remove(name))
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
            var names = _policies
                .Where(pair => string.Equals(pair.Value.ContributionId, contributionId, StringComparison.Ordinal))
                .Select(pair => pair.Key)
                .ToArray();

            foreach (var name in names)
            {
                _policies.Remove(name);
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
            return _policies.ContainsKey(name);
        }
    }

    public bool TryGet(
        string name,
        [NotNullWhen(true)] out AuthorizationPolicy? policy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_syncRoot)
        {
            return _policies.TryGetValue(name, out policy);
        }
    }

    public ValueTask<AuthorizationPolicy?> GetPolicyAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult<AuthorizationPolicy?>(null);
        }

        lock (_syncRoot)
        {
            _policies.TryGetValue(name, out var policy);

            return ValueTask.FromResult<AuthorizationPolicy?>(policy);
        }
    }
}
