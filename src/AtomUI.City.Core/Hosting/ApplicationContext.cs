namespace AtomUI.City.Hosting;

public sealed class ApplicationContext
{
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
}
