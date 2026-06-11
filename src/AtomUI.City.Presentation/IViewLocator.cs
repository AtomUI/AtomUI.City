namespace AtomUI.City.Presentation;

public interface IViewLocator
{
    bool TryLocate(
        Type viewModelType,
        string? viewKey,
        out ViewDescriptor? descriptor);

    ViewDescriptor Locate(Type viewModelType, string? viewKey = null);
}
