namespace AtomUI.City.Presentation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ViewForAttribute : Attribute
{
    public ViewForAttribute(Type viewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);

        ViewModelType = viewModelType;
    }

    public Type ViewModelType { get; }

    public string? Key { get; init; }

    public string? ContributionId { get; init; }
}
