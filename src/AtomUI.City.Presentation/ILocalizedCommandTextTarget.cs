namespace AtomUI.City.Presentation;

public interface ILocalizedCommandTextTarget
{
    string? Text { get; set; }

    string? ToolTip { get; set; }

    string? Description { get; set; }
}
