namespace AtomUI.City.Presentation;

public interface IActivePluginViewLease : IDisposable
{
    ActivePluginView View { get; }
}
