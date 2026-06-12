using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class LocalizedWindowTextBinding
{
    private readonly LocalizedTextBindingSet _bindingSet;

    public LocalizedWindowTextBinding(
        ILocalizationService localization,
        IUiDispatcher dispatcher)
    {
        _bindingSet = new LocalizedTextBindingSet(localization, dispatcher);
    }

    public async ValueTask<IDisposable> BindAsync(
        WindowTextDescriptor descriptor,
        ILocalizedWindowTextTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(target);

        var resources = new List<IDisposable>();

        try
        {
            if (descriptor.TitleKey is null)
            {
                await _bindingSet
                    .ApplyTextAsync(
                        value => target.Title = value,
                        descriptor.Title,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await _bindingSet
                    .BindKeyAsync(
                        descriptor.TitleKey,
                        value => target.Title = value,
                        resources,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            return LocalizedTextBindingSet.CreateHandle(resources);
        }
        catch
        {
            LocalizedTextBindingSet.DisposeAll(resources);
            throw;
        }
    }

    public async ValueTask<IDisposable> BindAsync(
        WindowTextDescriptor descriptor,
        ILocalizedWindowTextTarget target,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        var handle = await BindAsync(descriptor, target, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }
}
