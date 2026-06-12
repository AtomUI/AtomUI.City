namespace AtomUI.City.Presentation;

public interface IViewRegistry : IViewLocator
{
    void Register(ViewDescriptor descriptor);

    int RevokePlugin(string pluginId);

    int RevokeContribution(string contributionId);
}
