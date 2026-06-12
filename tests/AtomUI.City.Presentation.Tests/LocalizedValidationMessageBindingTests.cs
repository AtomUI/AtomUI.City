using System.Globalization;
using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class LocalizedValidationMessageBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesLocalizedValidationMessage()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Validation.Required", "请输入 {0}"));
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var dispatcher = new RecordingDispatcher();
        var binding = new LocalizedValidationMessageBinding(localization, dispatcher);
        var target = new ValidationMessageTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new ValidationMessage(
                "Name",
                "Name is required.",
                "Validation.Required",
                ["名称"]),
            target);

        Assert.Equal("请输入 名称", target.Message);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BoundValidationMessageRefreshesWhenCultureChanges()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Validation.Required", "请输入 {0}"));
        var en = Package("Host.en-US", "en-US", ("Validation.Required", "{0} is required."));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedValidationMessageBinding(localization, new RecordingDispatcher());
        var target = new ValidationMessageTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new ValidationMessage(
                "Name",
                "Name is required.",
                "Validation.Required",
                ["Name"]),
            target);
        await localization.SetCultureAsync("en-US");

        Assert.Equal("Name is required.", target.Message);
    }

    [Fact]
    public async Task BindAsyncUsesLiteralMessageWhenMessageKeyIsMissing()
    {
        var zh = Package("Host.zh-CN", "zh-CN");
        var localization = new LocalizationService(
            [zh.Descriptor],
            [new TestLanguagePackageProvider(zh)]);
        var dispatcher = new RecordingDispatcher();
        var binding = new LocalizedValidationMessageBinding(localization, dispatcher);
        var target = new ValidationMessageTarget();

        await localization.SetCultureAsync("zh-CN");
        using var handle = await binding.BindAsync(
            new ValidationMessage("Name", "Name is required."),
            target);

        Assert.Equal("Name is required.", target.Message);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task ActivationScopeDisposesValidationMessageBinding()
    {
        var zh = Package("Host.zh-CN", "zh-CN", ("Validation.Required", "请输入 {0}"));
        var en = Package("Host.en-US", "en-US", ("Validation.Required", "{0} is required."));
        var localization = new LocalizationService(
            [zh.Descriptor, en.Descriptor],
            [new TestLanguagePackageProvider(zh, en)]);
        var binding = new LocalizedValidationMessageBinding(localization, new RecordingDispatcher());
        var target = new ValidationMessageTarget();
        using var activationScope = new ActivationScope();

        await localization.SetCultureAsync("zh-CN");
        await binding.BindAsync(
            new ValidationMessage(
                "Name",
                "Name is required.",
                "Validation.Required",
                ["Name"]),
            target,
            activationScope);
        activationScope.Dispose();
        await localization.SetCultureAsync("en-US");

        Assert.Equal("请输入 Name", target.Message);
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

    private sealed class ValidationMessageTarget : ILocalizedValidationMessageTarget
    {
        public string? Message { get; set; }
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
