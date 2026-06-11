using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class LocalizedTextBinding
{
    private readonly IUiDispatcher _dispatcher;

    public LocalizedTextBinding(IUiDispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        _dispatcher = dispatcher;
    }

    public async ValueTask<IDisposable> BindAsync(
        ILocalizedText text,
        ILocalizedTextTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(target);

        await ApplyAsync(target, text.Value, cancellationToken).ConfigureAwait(false);

        var handle = new BindingHandle(this, text, target);
        text.Changed += handle.OnChanged;

        return handle;
    }

    public async ValueTask<IDisposable> BindAsync(
        ILocalizedText text,
        ILocalizedTextTarget target,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        var handle = await BindAsync(text, target, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }

    private ValueTask ApplyAsync(
        ILocalizedTextTarget target,
        string value,
        CancellationToken cancellationToken = default)
    {
        return _dispatcher.InvokeAsync(
            () =>
            {
                target.Text = value;
            },
            cancellationToken);
    }

    private sealed class BindingHandle : IDisposable
    {
        private readonly LocalizedTextBinding _binding;
        private readonly ILocalizedText _text;
        private readonly ILocalizedTextTarget _target;
        private bool _disposed;

        public BindingHandle(
            LocalizedTextBinding binding,
            ILocalizedText text,
            ILocalizedTextTarget target)
        {
            _binding = binding;
            _text = text;
            _target = target;
        }

        public void OnChanged(object? sender, LocalizedTextChangedEventArgs args)
        {
            ArgumentNullException.ThrowIfNull(args);

            if (_disposed)
            {
                return;
            }

            var update = _binding.ApplyAsync(_target, args.Value);
            if (!update.IsCompletedSuccessfully)
            {
                _ = ObserveAsync(update);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _text.Changed -= OnChanged;
        }

        private static async Task ObserveAsync(ValueTask update)
        {
            try
            {
                await update.ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }
}
