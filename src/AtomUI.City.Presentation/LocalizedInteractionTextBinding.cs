using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class LocalizedInteractionTextBinding
{
    private readonly LocalizedTextBindingSet _bindingSet;

    public LocalizedInteractionTextBinding(
        ILocalizationService localization,
        IUiDispatcher dispatcher)
    {
        _bindingSet = new LocalizedTextBindingSet(localization, dispatcher);
    }

    public async ValueTask<IDisposable> BindAsync(
        InteractionTextDescriptor descriptor,
        ILocalizedInteractionTextTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(target);

        var resources = new List<IDisposable>();

        try
        {
            await _bindingSet.BindKeyAsync(
                    descriptor.TitleKey,
                    value => target.Title = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);
            await _bindingSet.BindKeyAsync(
                    descriptor.MessageKey,
                    value => target.Message = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);
            await _bindingSet.BindKeyAsync(
                    descriptor.PrimaryActionKey,
                    value => target.PrimaryActionText = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);
            await _bindingSet.BindKeyAsync(
                    descriptor.SecondaryActionKey,
                    value => target.SecondaryActionText = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);
            await _bindingSet.BindKeyAsync(
                    descriptor.CancelActionKey,
                    value => target.CancelActionText = value,
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
        InteractionTextDescriptor descriptor,
        ILocalizedInteractionTextTarget target,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        var handle = await BindAsync(descriptor, target, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }
}
