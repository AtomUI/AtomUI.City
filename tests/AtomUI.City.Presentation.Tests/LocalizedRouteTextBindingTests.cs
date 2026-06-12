using System.Globalization;
using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Routing;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class LocalizedRouteTextBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesRouteMetadataText()
    {
        var zh = Package(
            "Host.zh-CN",
            "zh-CN",
            ("Routes.Settings.Title", "设置"),
            ("Routes.Settings.Description", "配置应用选项"),
            ("Routes.Settings.Breadcrumb", "设置"),
            ("Routes.Settings.Group", "系统"));
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var dispatcher = new RecordingDispatcher();
        var binding = new LocalizedRouteTextBinding(localization, dispatcher);
        var target = new RouteTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            Route("settings"),
            target);

        Assert.Equal("设置", target.Title);
        Assert.Equal("配置应用选项", target.Description);
        Assert.Equal("设置", target.Breadcrumb);
        Assert.Equal("系统", target.Group);
        Assert.Equal(4, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BoundRouteTextRefreshesWhenCultureChanges()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Routes.Settings.Title", "设置"));
        var en = Package("Host.en-US", "en-US", ("Routes.Settings.Title", "Settings"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedRouteTextBinding(localization, new RecordingDispatcher());
        var target = new RouteTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            Route("settings"),
            target);
        await localization.SetCultureAsync("en-US");

        Assert.Equal("Settings", target.Title);
    }

    [Fact]
    public async Task ActivationScopeDisposesRouteTextBinding()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Routes.Settings.Title", "设置"));
        var en = Package("Host.en-US", "en-US", ("Routes.Settings.Title", "Settings"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedRouteTextBinding(localization, new RecordingDispatcher());
        var target = new RouteTextTarget();
        using var activationScope = new ActivationScope();

        await localization.SetCultureAsync("zh-CN");
        await binding.BindAsync(
            Route("settings"),
            target,
            activationScope);
        activationScope.Dispose();
        await localization.SetCultureAsync("en-US");

        Assert.Equal("设置", target.Title);
    }

    private static RouteDescriptor Route(string routeId)
    {
        return new RouteDescriptor(
            routeId,
            RouteDefinitionKind.Route,
            "settings",
            new ViewModelTargetDescriptor(typeof(SettingsViewModel)),
            metadata: new RouteMetadataDescriptor(
                titleKey: "Routes.Settings.Title",
                descriptionKey: "Routes.Settings.Description",
                breadcrumbKey: "Routes.Settings.Breadcrumb",
                groupKey: "Routes.Settings.Group"));
    }

    private static LanguagePackage Package(
        string packageId,
        string cultureName,
        params (string Key, string Value)[] resources)
    {
        return LanguagePackage.Create(
            new LanguagePackageDescriptor(
                packageId,
                CultureInfo.GetCultureInfo(cultureName),
                ResourceScope.Host),
            resources.ToDictionary(resource => resource.Key, resource => resource.Value));
    }

    private sealed class RouteTextTarget : ILocalizedRouteTextTarget
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Breadcrumb { get; set; }

        public string? Group { get; set; }

        public string? ErrorTitle { get; set; }
    }

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public int InvokeCount { get; private set; }

        public bool CheckAccess() => true;

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
            InvokeCount++;

            return ValueTask.FromResult(callback());
        }

        public ValueTask PostAsync(
            Func<CancellationToken, ValueTask> callback,
            CancellationToken cancellationToken = default)
        {
            return callback(cancellationToken);
        }
    }

    private sealed class TestLanguagePackageProvider : ILanguagePackageProvider
    {
        private readonly Dictionary<string, LanguagePackage> _packages;

        public TestLanguagePackageProvider(params LanguagePackage[] packages)
        {
            _packages = packages.ToDictionary(package => package.Descriptor.PackageId, StringComparer.Ordinal);
        }

        public LanguagePackageProviderKind Kind => LanguagePackageProviderKind.InMemory;

        public ValueTask<LanguagePackageLoadResult> LoadAsync(
            LanguagePackageDescriptor descriptor,
            CancellationToken cancellationToken = default)
        {
            return _packages.TryGetValue(descriptor.PackageId, out var package)
                ? ValueTask.FromResult(LanguagePackageLoadResult.Success(package))
                : ValueTask.FromResult(
                    LanguagePackageLoadResult.Failed(
                        new LocalizationError(
                            LocalizationErrorKind.PackageNotFound,
                            "Package was not found.")));
        }
    }

    private sealed class SettingsViewModel;
}
