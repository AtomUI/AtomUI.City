namespace AtomUI.City.Presentation;

public interface IPresentationResourceRegistry
{
    IReadOnlyList<PresentationResourceContribution> Contributions { get; }

    IPresentationResourceLease Register(PresentationResourceContribution contribution);

    int RevokePlugin(string pluginId);

    int RevokeContribution(string contributionId);
}
