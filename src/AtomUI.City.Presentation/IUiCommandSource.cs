namespace AtomUI.City.Presentation;

public interface IUiCommandSource
{
    event EventHandler? ExecuteRequested;

    object? CommandParameter { get; }

    void ApplyCommandState(UiCommandState state);
}
