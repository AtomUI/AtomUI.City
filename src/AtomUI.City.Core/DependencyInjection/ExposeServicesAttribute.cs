namespace AtomUI.City.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ExposeServicesAttribute : Attribute
{
    private readonly Type[] _serviceTypes;

    public ExposeServicesAttribute(params Type[] serviceTypes)
    {
        _serviceTypes = (serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes))).ToArray();
    }

    public Type[] ServiceTypes => _serviceTypes.ToArray();
}
