using System.Globalization;
using AtomUI.City.Localization;

namespace AtomUI.City.Presentation;

public sealed class CurrentThreadCultureApplier : IPresentationCultureApplier
{
    public ValueTask<LocalizationResult> ApplyCultureAsync(
        CultureState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        cancellationToken.ThrowIfCancellationRequested();

        CultureInfo.CurrentCulture = state.CurrentCulture;
        CultureInfo.CurrentUICulture = state.CurrentUICulture;
        CultureInfo.DefaultThreadCurrentCulture = state.CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = state.CurrentUICulture;

        return ValueTask.FromResult(LocalizationResult.Success());
    }
}
