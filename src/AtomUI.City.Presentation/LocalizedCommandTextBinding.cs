using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class LocalizedCommandTextBinding
{
    private readonly LocalizedTextBindingSet _bindingSet;

    public LocalizedCommandTextBinding(
        ILocalizationService localization,
        IUiDispatcher dispatcher)
    {
        _bindingSet = new LocalizedTextBindingSet(localization, dispatcher);
    }

    public async ValueTask<IDisposable> BindAsync(
        CommandTextDescriptor descriptor,
        ILocalizedCommandTextTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(target);

        var resources = new List<IDisposable>();

        try
        {
            await _bindingSet.BindKeyAsync(
                    descriptor.TextKey,
                    value => target.Text = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);
            await _bindingSet.BindKeyAsync(
                    descriptor.ToolTipKey,
                    value => target.ToolTip = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);
            await _bindingSet.BindKeyAsync(
                    descriptor.DescriptionKey,
                    value => target.Description = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);

            return LocalizedTextBindingSet.CreateHandle(resources);
        }
        catch
        {
            LocalizedTextBindingSet.DisposeAll(resources);
            throw;
        }
    }

    public async ValueTask<IDisposable> BindAsync(
        CommandTextDescriptor descriptor,
        ILocalizedCommandTextTarget target,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        var handle = await BindAsync(descriptor, target, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }
}
