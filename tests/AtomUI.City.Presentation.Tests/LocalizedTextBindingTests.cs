using System.Globalization;
using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Presentation;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation.Tests;

public sealed class LocalizedTextBindingTests
{
    [Fact]
    public async Task BindAsyncAppliesInitialTextOnUiDispatcher()
    {
        var dispatcher = new RecordingDispatcher();
        var target = new TextTarget();
        var text = new ControllableLocalizedText("Settings.Title", "设置", "zh-CN", revision: 1);
        var binding = new LocalizedTextBinding(dispatcher);

        using var handle = await binding.BindAsync(text, target);

        Assert.Equal("设置", target.Text);
        Assert.Equal(1, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task BoundTextTargetRefreshesWhenLocalizedTextChanges()
    {
        var dispatcher = new RecordingDispatcher();
        var target = new TextTarget();
        var text = new ControllableLocalizedText("Settings.Title", "设置", "zh-CN", revision: 1);
        var binding = new LocalizedTextBinding(dispatcher);

        using var handle = await binding.BindAsync(text, target);
        text.SetValue("Settings", "en-US", revision: 2);

        Assert.Equal("Settings", target.Text);
        Assert.Equal(2, dispatcher.InvokeCount);
    }

    [Fact]
    public async Task DisposedBindingStopsRefreshingTarget()
    {
        var dispatcher = new RecordingDispatcher();
        var target = new TextTarget();
        var text = new ControllableLocalizedText("Settings.Title", "设置", "zh-CN", revision: 1);
        var binding = new LocalizedTextBinding(dispatcher);

        var handle = await binding.BindAsync(text, target);
        handle.Dispose();
        text.SetValue("Settings", "en-US", revision: 2);

        Assert.Equal("设置", target.Text);
    }

    [Fact]
    public async Task BindAsyncRegistersBindingWithActivationScope()
    {
        var dispatcher = new RecordingDispatcher();
        var target = new TextTarget();
        var text = new ControllableLocalizedText("Settings.Title", "设置", "zh-CN", revision: 1);
        var binding = new LocalizedTextBinding(dispatcher);
        using var activationScope = new ActivationScope();

        await binding.BindAsync(text, target, activationScope);
        activationScope.Dispose();
        text.SetValue("Settings", "en-US", revision: 2);

        Assert.Equal("设置", target.Text);
    }

    private sealed class TextTarget : ILocalizedTextTarget
    {
        public string? Text { get; set; }
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

    private sealed class ControllableLocalizedText : ILocalizedText
    {
        public ControllableLocalizedText(
            string key,
            string value,
            string cultureName,
            long revision)
        {
            Key = key;
            Value = value;
            Culture = CultureInfo.GetCultureInfo(cultureName);
            Revision = revision;
        }

        public event EventHandler<LocalizedTextChangedEventArgs>? Changed;

        public string Key { get; private set; }

        public string Value { get; private set; }

        public CultureInfo Culture { get; private set; }

        public long Revision { get; private set; }

        public bool IsFallback { get; private set; }

        public bool IsMissing { get; private set; }

        public ValueTask RefreshAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }

        public void SetValue(
            string value,
            string cultureName,
            long revision)
        {
            Value = value;
            Culture = CultureInfo.GetCultureInfo(cultureName);
            Revision = revision;
            Changed?.Invoke(
                this,
                new LocalizedTextChangedEventArgs(
                    LocalizedString.Found(Key, Value, Culture),
                    revision));
        }

        public void Dispose()
        {
        }
    }
}
