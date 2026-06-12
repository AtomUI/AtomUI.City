using System.Globalization;
using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class LocalizedWindowTextBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesWindowTitle()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Windows.Main.Title", "订单工作台"));
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var dispatcher = new RecordingDispatcher();
        var binding = new LocalizedWindowTextBinding(localization, dispatcher);
        var target = new WindowTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new WindowTextDescriptor("main", titleKey: "Windows.Main.Title"),
            target);

        Assert.Equal("订单工作台", target.Title);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BoundWindowTitleRefreshesWhenCultureChanges()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Windows.Main.Title", "订单工作台"));
        var en = Package("Host.en-US", "en-US", ("Windows.Main.Title", "Orders"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedWindowTextBinding(localization, new RecordingDispatcher());
        var target = new WindowTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new WindowTextDescriptor("main", titleKey: "Windows.Main.Title"),
            target);
        await localization.SetCultureAsync("en-US");

        Assert.Equal("Orders", target.Title);
    }

    [Fact]
    public async Task BindAsyncUsesLiteralTitleWhenTitleKeyIsMissing()
    {
        var zh = Package("Host.zh-CN", "zh-CN");
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var binding = new LocalizedWindowTextBinding(localization, new RecordingDispatcher());
        var target = new WindowTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new WindowTextDescriptor("main", title: "Orders"),
            target);

        Assert.Equal("Orders", target.Title);
    }

    [Fact]
    public async Task ActivationScopeDisposesWindowTitleBinding()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Windows.Main.Title", "订单工作台"));
        var en = Package("Host.en-US", "en-US", ("Windows.Main.Title", "Orders"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedWindowTextBinding(localization, new RecordingDispatcher());
        var target = new WindowTextTarget();
        using var activationScope = new ActivationScope();

        await localization.SetCultureAsync("zh-CN");
        await binding.BindAsync(
            new WindowTextDescriptor("main", titleKey: "Windows.Main.Title"),
            target,
            activationScope);
        activationScope.Dispose();
        await localization.SetCultureAsync("en-US");

        Assert.Equal("订单工作台", target.Title);
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

    private sealed class WindowTextTarget : ILocalizedWindowTextTarget
    {
        public string? Title { get; set; }
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
}
