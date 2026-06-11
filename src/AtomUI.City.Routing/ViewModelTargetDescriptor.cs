namespace AtomUI.City.Routing;

public sealed class ViewModelTargetDescriptor
{
    public ViewModelTargetDescriptor(Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        ViewModelType = viewModelType;
    }

    public Type ViewModelType { get; }
}
