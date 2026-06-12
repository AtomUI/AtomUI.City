namespace AtomUI.City.Presentation;

public interface ILocalizedRouteTextTarget
{
    string? Title { get; set; }

    string? Description { get; set; }

    string? Breadcrumb { get; set; }

    string? Group { get; set; }

    string? ErrorTitle { get; set; }
}
