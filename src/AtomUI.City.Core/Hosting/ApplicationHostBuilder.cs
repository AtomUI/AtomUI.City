using AtomUI.City.Diagnostics;
using AtomUI.City.Lifecycle;
using AtomUI.City.Modularity;
using AtomUI.City.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AtomUI.City.Hosting;

public sealed class ApplicationHostBuilder : IApplicationHostBuilder
{
    private readonly string[] _args;
    private readonly HostApplicationBuilder _builder;
    private readonly List<Action<ApplicationHostOptions>> _configureHostActions = [];
    private bool _built;

    internal ApplicationHostBuilder(string[] args)
    {
        _args = args.ToArray();
        _builder = Host.CreateApplicationBuilder(_args);
    }

    public IServiceCollection Services => _builder.Services;

    public IConfigurationManager Configuration => _builder.Configuration;

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>(StringComparer.Ordinal);

    public IApplicationHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
    {
        ArgumentNullException.ThrowIfNull(configureServices);

        configureServices(Services);

        return this;
    }

    public IApplicationHostBuilder ConfigureHost(Action<ApplicationHostOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        _configureHostActions.Add(configureOptions);

        return this;
    }

    public IApplicationHost Build()
    {
        if (_built)
        {
            throw new InvalidOperationException("Application host builder can only build once.");
        }

        _built = true;

        var hostOptions = CreateHostOptions();
        var context = CreateApplicationContext(hostOptions);
        var moduleRegistry = ModuleRegistry.Create(ModuleRegistrationStore.GetRegistrations(this));

        Services.AddSingleton(context);
        Services.AddSingleton<IApplicationContext>(context);
        Services.AddSingleton(Options.Create(hostOptions));
        Services.TryAddSingleton<IHostDiagnostics, InMemoryHostDiagnostics>();
        Services.TryAddSingleton<IUiDispatcher, UnavailableUiDispatcher>();
        Services.TryAddSingleton<IModuleRegistry>(moduleRegistry);

        moduleRegistry.ConfigureServicesAsync(context, Services).AsTask().GetAwaiter().GetResult();

        var genericHost = _builder.Build();

        context.Services = genericHost.Services;
        var diagnostics = genericHost.Services.GetRequiredService<IHostDiagnostics>();

        diagnostics.Write(new HostDiagnosticRecord(
            HostDiagnosticIds.HostBuilt,
            "Application host has been built.",
            HostDiagnosticSeverity.Info));

        return new DefaultApplicationHost(
            genericHost,
            context,
            diagnostics,
            LifecycleScope.CreateRoot(LifecycleScopeKind.Host, "host"),
            moduleRegistry);
    }

    private ApplicationHostOptions CreateHostOptions()
    {
        var options = new ApplicationHostOptions();

        foreach (var configure in _configureHostActions)
        {
            configure(options);
        }

        return options;
    }

    private ApplicationContext CreateApplicationContext(ApplicationHostOptions hostOptions)
    {
        var applicationName = !string.IsNullOrWhiteSpace(hostOptions.ApplicationName)
            ? hostOptions.ApplicationName
            : string.IsNullOrWhiteSpace(_builder.Environment.ApplicationName)
            ? "AtomUI.City.Application"
            : _builder.Environment.ApplicationName;
        var context = new ApplicationContext
        {
            ApplicationName = applicationName,
            EnvironmentName = _builder.Environment.EnvironmentName,
            ContentRootPath = _builder.Environment.ContentRootPath,
            AppDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                applicationName),
            StartupArguments = _args,
            Configuration = _builder.Configuration,
        };

        foreach (var property in Properties)
        {
            context.Properties[property.Key] = property.Value;
        }

        return context;
    }
}
