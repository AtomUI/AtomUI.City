using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class LocalizedNotificationTextBinding
{
    private readonly LocalizedTextBindingSet _bindingSet;

    public LocalizedNotificationTextBinding(
        ILocalizationService localization,
        IUiDispatcher dispatcher)
    {
        _bindingSet = new LocalizedTextBindingSet(localization, dispatcher);
    }

    public async ValueTask<IDisposable> BindAsync(
        NotificationTextDescriptor descriptor,
        ILocalizedNotificationTextTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(target);

        var resources = new List<IDisposable>();

        try
        {
            await BindTextAsync(
                    descriptor.TitleKey,
                    descriptor.Title,
                    value => target.Title = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);
            await _bindingSet.BindMessageAsync(
                    descriptor.MessageKey,
                    descriptor.MessageArguments,
                    descriptor.Message,
                    value => target.Message = value,
                    resources,
                    cancellationToken)
                .ConfigureAwait(false);
            await BindTextAsync(
                    descriptor.ActionTextKey,
                    descriptor.ActionText,
                    value => target.ActionText = value,
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
        NotificationTextDescriptor descriptor,
        ILocalizedNotificationTextTarget target,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        var handle = await BindAsync(descriptor, target, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }

    private async ValueTask BindTextAsync(
        string? key,
        string? fallbackText,
        Action<string?> setText,
        List<IDisposable> resources,
        CancellationToken cancellationToken)
    {
        if (key is null)
        {
            await _bindingSet
                .ApplyTextAsync(setText, fallbackText, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        await _bindingSet
            .BindKeyAsync(key, setText, resources, cancellationToken)
            .ConfigureAwait(false);
    }
}
