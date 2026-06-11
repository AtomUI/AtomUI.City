namespace AtomUI.City.Hosting;

public static class ApplicationHost
{
    public static IApplicationHostBuilder CreateBuilder(string[]? args = null)
    {
        return new ApplicationHostBuilder(args ?? []);
    }
}
