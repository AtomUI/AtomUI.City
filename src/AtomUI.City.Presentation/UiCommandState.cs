namespace AtomUI.City.Presentation;

public sealed record UiCommandState(
    bool CanExecute,
    bool IsExecuting);
