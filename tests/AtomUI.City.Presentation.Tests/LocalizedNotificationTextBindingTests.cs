using System.Globalization;
using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class LocalizedNotificationTextBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesNotificationText()
    {
        var zh = Package(
            "Host.zh-CN",
            "zh-CN",
            ("Notifications.Sync.Title", "同步完成"),
            ("Notifications.Sync.Message", "已同步 {0} 条记录"),
            ("Notifications.Sync.Action", "查看"));
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var dispatcher = new RecordingDispatcher();
        var binding = new LocalizedNotificationTextBinding(localization, dispatcher);
        var target = new NotificationTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new NotificationTextDescriptor(
                "sync-completed",
                titleKey: "Notifications.Sync.Title",
                messageKey: "Notifications.Sync.Message",
                messageArguments: [12],
                actionTextKey: "Notifications.Sync.Action"),
            target);

        Assert.Equal("同步完成", target.Title);
        Assert.Equal("已同步 12 条记录", target.Message);
        Assert.Equal("查看", target.ActionText);
        Assert.Equal(3, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BoundNotificationMessageRefreshesWhenCultureChanges()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Notifications.Sync.Message", "已同步 {0} 条记录"));
        var en = Package("Host.en-US", "en-US", ("Notifications.Sync.Message", "{0} records synced."));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedNotificationTextBinding(localization, new RecordingDispatcher());
        var target = new NotificationTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new NotificationTextDescriptor(
                "sync-completed",
                messageKey: "Notifications.Sync.Message",
                messageArguments: [12]),
            target);
        await localization.SetCultureAsync("en-US");

        Assert.Equal("12 records synced.", target.Message);
    }

    [Fact]
    public async Task BindAsyncUsesLiteralNotificationTextWhenKeysAreMissing()
    {
        var zh = Package("Host.zh-CN", "zh-CN");
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var binding = new LocalizedNotificationTextBinding(localization, new RecordingDispatcher());
        var target = new NotificationTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new NotificationTextDescriptor(
                "sync-completed",
                title: "Sync completed",
                message: "12 records synced.",
                actionText: "View"),
            target);

        Assert.Equal("Sync completed", target.Title);
        Assert.Equal("12 records synced.", target.Message);
        Assert.Equal("View", target.ActionText);
    }

    [Fact]
    public async Task ActivationScopeDisposesNotificationTextBinding()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Notifications.Sync.Title", "同步完成"));
        var en = Package("Host.en-US", "en-US", ("Notifications.Sync.Title", "Sync completed"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedNotificationTextBinding(localization, new RecordingDispatcher());
        var target = new NotificationTextTarget();
        using var activationScope = new ActivationScope();

        await localization.SetCultureAsync("zh-CN");
        await binding.BindAsync(
            new NotificationTextDescriptor("sync-completed", titleKey: "Notifications.Sync.Title"),
            target,
            activationScope);
        activationScope.Dispose();
        await localization.SetCultureAsync("en-US");

        Assert.Equal("同步完成", target.Title);
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

    private sealed class NotificationTextTarget : ILocalizedNotificationTextTarget
    {
        public string? Title { get; set; }

        public string? Message { get; set; }

        public string? ActionText { get; set; }
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
