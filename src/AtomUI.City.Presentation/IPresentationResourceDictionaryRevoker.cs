using AtomUI.City.Localization;

namespace AtomUI.City.Presentation;

public interface IPresentationResourceDictionaryRevoker
{
    ValueTask<LocalizationResult> RevokeAsync(
        PresentationResourceDictionaryRevocation revocation,
        CancellationToken cancellationToken = default);
}
