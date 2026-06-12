namespace AtomUI.City.Presentation;

public interface IPresentationResourceLease : IDisposable
{
    PresentationResourceContribution Contribution { get; }
}
