using System.Diagnostics.CodeAnalysis;

namespace AtomUI.City.Security;

public interface IAuthorizationPolicyProvider
{
    long Revision { get; }

    IReadOnlyCollection<AuthorizationPolicy> Policies { get; }

    bool Contains(string name);

    bool TryGet(
        string name,
        [NotNullWhen(true)] out AuthorizationPolicy? policy);

    ValueTask<AuthorizationPolicy?> GetPolicyAsync(
        string name,
        CancellationToken cancellationToken = default);
}
