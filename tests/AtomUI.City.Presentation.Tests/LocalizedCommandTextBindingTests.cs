using System.Globalization;
using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class LocalizedCommandTextBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesCommandTextMetadata()
    {
        var zh = Package(
            "Host.zh-CN",
            "zh-CN",
            ("Commands.Save.Text", "保存"),
            ("Commands.Save.ToolTip", "保存当前文档"),
            ("Commands.Save.Description", "将当前文档写入磁盘"));
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var dispatcher = new RecordingDispatcher();
        var binding = new LocalizedCommandTextBinding(localization, dispatcher);
        var target = new CommandTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new CommandTextDescriptor(
                "document.save",
                textKey: "Commands.Save.Text",
                toolTipKey: "Commands.Save.ToolTip",
                descriptionKey: "Commands.Save.Description"),
            target);

        Assert.Equal("保存", target.Text);
        Assert.Equal("保存当前文档", target.ToolTip);
        Assert.Equal("将当前文档写入磁盘", target.Description);
        Assert.Equal(3, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BoundCommandTextRefreshesWhenCultureChanges()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Commands.Save.Text", "保存"));
        var en = Package("Host.en-US", "en-US", ("Commands.Save.Text", "Save"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedCommandTextBinding(localization, new RecordingDispatcher());
        var target = new CommandTextTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new CommandTextDescriptor("document.save", textKey: "Commands.Save.Text"),
            target);
        await localization.SetCultureAsync("en-US");

        Assert.Equal("Save", target.Text);
    }

    [Fact]
    public async Task ActivationScopeDisposesCommandTextBinding()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Commands.Save.Text", "保存"));
        var en = Package("Host.en-US", "en-US", ("Commands.Save.Text", "Save"));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedCommandTextBinding(localization, new RecordingDispatcher());
        var target = new CommandTextTarget();
        using var activationScope = new ActivationScope();

        await localization.SetCultureAsync("zh-CN");
        await binding.BindAsync(
            new CommandTextDescriptor("document.save", textKey: "Commands.Save.Text"),
            target,
            activationScope);
        activationScope.Dispose();
        await localization.SetCultureAsync("en-US");

        Assert.Equal("保存", target.Text);
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

    private sealed class CommandTextTarget : ILocalizedCommandTextTarget
    {
        public string? Text { get; set; }

        public string? ToolTip { get; set; }

        public string? Description { get; set; }
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
