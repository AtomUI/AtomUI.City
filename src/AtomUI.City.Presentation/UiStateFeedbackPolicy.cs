namespace AtomUI.City.Presentation;

public static class UiStateFeedbackPolicy
{
    public static bool CanNotifyViewModel(UiStateFeedbackKind kind)
    {
        return kind is
            UiStateFeedbackKind.ValueChanged or
            UiStateFeedbackKind.CommandInvoked or
            UiStateFeedbackKind.InteractionCompleted or
            UiStateFeedbackKind.ValidationRequested or
            UiStateFeedbackKind.SelectionChanged;
    }
}
