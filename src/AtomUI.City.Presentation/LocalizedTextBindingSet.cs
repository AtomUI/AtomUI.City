using AtomUI.City.Localization;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

internal sealed class LocalizedTextBindingSet
{
    private readonly ILocalizationService _localization;
    private readonly LocalizedTextBinding _textBinding;

    public LocalizedTextBindingSet(
        ILocalizationService localization,
        IUiDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(localization);
        ArgumentNullException.ThrowIfNull(dispatcher);

        _localization = localization;
        _textBinding = new LocalizedTextBinding(dispatcher);
    }

    public async ValueTask BindKeyAsync(
        string? key,
        Action<string?> setText,
        List<IDisposable> resources,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(setText);
        ArgumentNullException.ThrowIfNull(resources);

        if (key is null)
        {
            return;
        }

        var text = await _localization.CreateTextAsync(key, cancellationToken).ConfigureAwait(false);
        resources.Add(text);
        var binding = await _textBinding
            .BindAsync(text, new TargetAdapter(setText), cancellationToken)
            .ConfigureAwait(false);
        resources.Add(binding);
    }

    public static IDisposable CreateHandle(List<IDisposable> resources)
    {
        ArgumentNullException.ThrowIfNull(resources);

        return new BindingHandle(resources);
    }

    public static void DisposeAll(List<IDisposable> resources)
    {
        for (var i = resources.Count - 1; i >= 0; i--)
        {
            resources[i].Dispose();
        }

        resources.Clear();
    }

    private sealed class TargetAdapter : ILocalizedTextTarget
    {
        private readonly Action<string?> _setText;

        public TargetAdapter(Action<string?> setText)
        {
            _setText = setText;
        }

        public string? Text
        {
            get => null;
            set => _setText(value);
        }
    }

    private sealed class BindingHandle : IDisposable
    {
        private List<IDisposable>? _resources;

        public BindingHandle(List<IDisposable> resources)
        {
            _resources = resources;
        }

        public void Dispose()
        {
            var resources = _resources;
            if (resources is null)
            {
                return;
            }

            _resources = null;
            DisposeAll(resources);
        }
    }
}
