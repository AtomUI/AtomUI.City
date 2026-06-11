namespace AtomUI.City.Presentation;

public interface ILocalizedInteractionTextTarget
{
    string? Title { get; set; }

    string? Message { get; set; }

    string? PrimaryActionText { get; set; }

    string? SecondaryActionText { get; set; }

    string? CancelActionText { get; set; }
}
