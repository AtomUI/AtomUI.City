using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class LocalizedValidationMessageBinding
{
    private readonly LocalizedTextBindingSet _bindingSet;

    public LocalizedValidationMessageBinding(
        ILocalizationService localization,
        IUiDispatcher dispatcher)
    {
        _bindingSet = new LocalizedTextBindingSet(localization, dispatcher);
    }

    public async ValueTask<IDisposable> BindAsync(
        ValidationMessage message,
        ILocalizedValidationMessageTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(target);

        var resources = new List<IDisposable>();

        try
        {
            await _bindingSet.BindMessageAsync(
                    message.MessageKey,
                    message.MessageArguments,
                    message.Message,
                    value => target.Message = value,
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
        ValidationMessage message,
        ILocalizedValidationMessageTarget target,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        var handle = await BindAsync(message, target, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }
}
