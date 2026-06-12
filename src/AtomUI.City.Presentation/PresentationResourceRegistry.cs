using AtomUI.City.Diagnostics;

namespace AtomUI.City.Presentation;

public sealed class PresentationResourceRegistry : IPresentationResourceRegistry
{
    private readonly object _gate = new();
    private readonly List<PresentationResourceLease> _leases = [];
    private readonly IHostDiagnostics? _diagnostics;

    public PresentationResourceRegistry()
    {
    }

    public PresentationResourceRegistry(IHostDiagnostics? diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public IReadOnlyList<PresentationResourceContribution> Contributions
    {
        get
        {
            lock (_gate)
            {
                return _leases
                    .Where(static lease => !lease.IsDisposed)
                    .Select(static lease => lease.Contribution)
                    .ToArray();
            }
        }
    }

    public IPresentationResourceLease Register(PresentationResourceContribution contribution)
    {
        ArgumentNullException.ThrowIfNull(contribution);

        var lease = new PresentationResourceLease(this, contribution);

        lock (_gate)
        {
            _leases.Add(lease);
        }

        WriteRegisteredDiagnostic(contribution);

        return lease;
    }

    public int RevokePlugin(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        return Revoke(contribution => string.Equals(contribution.PluginId, pluginId, StringComparison.Ordinal));
    }

    public int RevokeContribution(string contributionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contributionId);

        return Revoke(
            contribution => string.Equals(contribution.ContributionId, contributionId, StringComparison.Ordinal));
    }

    private int Revoke(Func<PresentationResourceContribution, bool> predicate)
    {
        PresentationResourceLease[] leases;

        lock (_gate)
        {
            leases = _leases
                .Where(lease => !lease.IsDisposed && predicate(lease.Contribution))
                .ToArray();
        }

        foreach (var lease in leases)
        {
            lease.Dispose();
        }

        return leases.Length;
    }

    private void Revoke(PresentationResourceLease lease)
    {
        if (!lease.MarkDisposed())
        {
            return;
        }

        lock (_gate)
        {
            _leases.Remove(lease);
        }

        try
        {
            if (lease.Contribution.Resource is IDisposable disposable)
            {
                disposable.Dispose();
            }

            WriteRevokedDiagnostic(lease.Contribution);
        }
        catch (Exception exception)
        {
            WriteRevokeFailedDiagnostic(lease.Contribution, exception);
        }
    }

    private void WriteRegisteredDiagnostic(PresentationResourceContribution contribution)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ResourceContributionRegistered,
            $"Presentation resource contribution registered kind '{contribution.Kind}' plugin '{Normalize(contribution.PluginId)}' contribution '{Normalize(contribution.ContributionId)}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteRevokedDiagnostic(PresentationResourceContribution contribution)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ResourceContributionRevoked,
            $"Presentation resource contribution revoked kind '{contribution.Kind}' plugin '{Normalize(contribution.PluginId)}' contribution '{Normalize(contribution.ContributionId)}'.",
            HostDiagnosticSeverity.Info));
    }

    private void WriteRevokeFailedDiagnostic(
        PresentationResourceContribution contribution,
        Exception exception)
    {
        _diagnostics?.Write(new HostDiagnosticRecord(
            PresentationDiagnosticIds.ResourceContributionRevokeFailed,
            $"Presentation resource contribution failed to revoke kind '{contribution.Kind}' plugin '{Normalize(contribution.PluginId)}' contribution '{Normalize(contribution.ContributionId)}': {exception.Message}",
            HostDiagnosticSeverity.Error));
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "<none>" : value;
    }

    private sealed class PresentationResourceLease : IPresentationResourceLease
    {
        private readonly PresentationResourceRegistry _registry;

        public PresentationResourceLease(
            PresentationResourceRegistry registry,
            PresentationResourceContribution contribution)
        {
            _registry = registry;
            Contribution = contribution;
        }

        public PresentationResourceContribution Contribution { get; }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            _registry.Revoke(this);
        }

        public bool MarkDisposed()
        {
            if (IsDisposed)
            {
                return false;
            }

            IsDisposed = true;

            return true;
        }
    }
}
