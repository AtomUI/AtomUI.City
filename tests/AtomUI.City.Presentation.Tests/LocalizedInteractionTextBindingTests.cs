using System.Globalization;
using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class LocalizedInteractionTextBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesInteractionTextMetadata()
    {
        var zh = Package(
            "Host.zh-CN",
            "zh-CN",
            ("Interactions.Delete.Title", "删除文档"),
            ("Interactions.Delete.Message", "确定要删除当前文档吗？"),
            ("Interactions.Delete.Primary", "删除"),
            ("Interactions.Delete.Cancel", "取消"));
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var binding = new LocalizedInteractionTextBinding(localization, new RecordingDispatcher());
        var target = new InteractionTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new InteractionTextDescriptor(
                titleKey: "Interactions.Delete.Title",
                messageKey: "Interactions.Delete.Message",
                primaryActionKey: "Interactions.Delete.Primary",
                cancelActionKey: "Interactions.Delete.Cancel"),
            target);

        Assert.Equal("删除文档", target.Title);
        Assert.Equal("确定要删除当前文档吗？", target.Message);
        Assert.Equal("删除", target.PrimaryActionText);
        Assert.Equal("取消", target.CancelActionText);
    }

    [Fact]
    public async Task BoundInteractionTextRefreshesWhenCultureChanges()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Interactions.Delete.Message", "确定要删除当前文档吗？"));
        var en = Package("Host.en-US", "en-US", ("Interactions.Delete.Message", "Delete the current document?"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedInteractionTextBinding(localization, new RecordingDispatcher());
        var target = new InteractionTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new InteractionTextDescriptor(messageKey: "Interactions.Delete.Message"),
            target);
        await localization.SetCultureAsync("en-US");

        Assert.Equal("Delete the current document?", target.Message);
    }

    [Fact]
    public async Task ActivationScopeDisposesInteractionTextBinding()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Interactions.Delete.Message", "确定要删除当前文档吗？"));
        var en = Package("Host.en-US", "en-US", ("Interactions.Delete.Message", "Delete the current document?"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedInteractionTextBinding(localization, new RecordingDispatcher());
        var target = new InteractionTextTarget();
        using var activationScope = new ActivationScope();

        await localization.SetCultureAsync("zh-CN");
        await binding.BindAsync(
            new InteractionTextDescriptor(messageKey: "Interactions.Delete.Message"),
            target,
            activationScope);
        activationScope.Dispose();
        await localization.SetCultureAsync("en-US");

        Assert.Equal("确定要删除当前文档吗？", target.Message);
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

    private sealed class InteractionTextTarget : ILocalizedInteractionTextTarget
    {
        public string? Title { get; set; }

        public string? Message { get; set; }

        public string? PrimaryActionText { get; set; }

        public string? SecondaryActionText { get; set; }

        public string? CancelActionText { get; set; }
    }

    private sealed class RecordingDispatcher : IUiDispatcher
    {
        public bool CheckAccess() => true;

        public ValueTask InvokeAsync(Action callback, CancellationToken cancellationToken = default)
        {
            callback();

            return ValueTask.CompletedTask;
        }

        public ValueTask<T> InvokeAsync<T>(Func<T> callback, CancellationToken cancellationToken = default)
        {
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
