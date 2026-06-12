namespace AtomUI.City.Presentation;

public interface ILocalizedNotificationTextTarget
{
    string? Title { get; set; }

    string? Message { get; set; }

    string? ActionText { get; set; }
}
