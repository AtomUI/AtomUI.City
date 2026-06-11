namespace AtomUI.City.Localization;

public interface ILocalizationDiagnostics
{
    IReadOnlyList<LocalizationDiagnosticRecord> Records { get; }

    void Write(LocalizationDiagnosticRecord record);
}
