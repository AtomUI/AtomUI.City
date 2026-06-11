namespace AtomUI.City.Hosting;

public sealed class ApplicationHostOptions
{
    public string? ApplicationName { get; set; }

    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public bool AllowDynamicDiscovery { get; set; }
}
