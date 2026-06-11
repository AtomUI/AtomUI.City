namespace AtomUI.City.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ExposeServicesAttribute : Attribute
{
    public ExposeServicesAttribute(params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes ?? throw new ArgumentNullException(nameof(serviceTypes));
    }

    public Type[] ServiceTypes { get; }
}
