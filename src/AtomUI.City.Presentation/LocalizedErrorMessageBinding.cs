using AtomUI.City.Localization;
using AtomUI.City.Mvvm;
using AtomUI.City.Security;
using AtomUI.City.Threading;

namespace AtomUI.City.Presentation;

public sealed class LocalizedErrorMessageBinding
{
    private readonly LocalizedTextBindingSet _bindingSet;

    public LocalizedErrorMessageBinding(
        ILocalizationService localization,
        IUiDispatcher dispatcher)
    {
        _bindingSet = new LocalizedTextBindingSet(localization, dispatcher);
    }

    public async ValueTask<IDisposable> BindAsync(
        ErrorMessageDescriptor descriptor,
        ILocalizedErrorMessageTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(target);

        var resources = new List<IDisposable>();

        try
        {
            await _bindingSet.BindMessageAsync(
                    descriptor.MessageKey,
                    descriptor.MessageArguments,
                    descriptor.Message,
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
        ErrorMessageDescriptor descriptor,
        ILocalizedErrorMessageTarget target,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(activationScope);

        var handle = await BindAsync(descriptor, target, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }

    public ValueTask<IDisposable> BindAsync(
        AuthorizationResult authorization,
        ILocalizedErrorMessageTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        return BindAsync(CreateDescriptor(authorization), target, cancellationToken);
    }

    public async ValueTask<IDisposable> BindAsync(
        AuthorizationResult authorization,
        ILocalizedErrorMessageTarget target,
        IActivationScope activationScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        var handle = await BindAsync(authorization, target, cancellationToken).ConfigureAwait(false);
        activationScope.Add(handle);

        return handle;
    }

    private static ErrorMessageDescriptor CreateDescriptor(AuthorizationResult authorization)
    {
        var errorCode = authorization.FailedRequirement is null
            ? $"security.{authorization.Status}"
            : $"security.{authorization.Status}.{authorization.FailedRequirement}";

        return new ErrorMessageDescriptor(
            errorCode,
            authorization.Message,
            authorization.MessageKey,
            authorization.MessageArguments);
    }
}
