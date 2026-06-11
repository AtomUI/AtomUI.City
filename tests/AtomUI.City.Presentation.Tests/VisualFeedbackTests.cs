using AtomUI.City.Presentation;

namespace AtomUI.City.Presentation.Tests;

public sealed class VisualFeedbackTests
{
    [Fact]
    public void VisualLifecycleHubPublishesNormalizedVisualTreeEvents()
    {
        var hub = new VisualLifecycleHub();
        var events = new List<VisualLifecycleEvent>();
        var view = new SettingsView();
        using var subscription = hub.Subscribe(events.Add);

        hub.Notify(view, VisualLifecycleEventKind.Attached);
        hub.Notify(view, VisualLifecycleEventKind.Detached);

        Assert.Collection(
            events,
            item =>
            {
                Assert.Same(view, item.View);
                Assert.Equal(VisualLifecycleEventKind.Attached, item.Kind);
            },
            item =>
            {
                Assert.Same(view, item.View);
                Assert.Equal(VisualLifecycleEventKind.Detached, item.Kind);
            });
    }

    [Fact]
    public void DisposedVisualLifecycleSubscriptionStopsReceivingEvents()
    {
        var hub = new VisualLifecycleHub();
        var events = new List<VisualLifecycleEvent>();
        using var subscription = hub.Subscribe(events.Add);

        subscription.Dispose();
        hub.Notify(new SettingsView(), VisualLifecycleEventKind.Attached);

        Assert.Empty(events);
    }

    [Fact]
    public void UiStateFeedbackPolicyOnlyAllowsSemanticFeedbackToViewModel()
    {
        Assert.True(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.ValueChanged));
        Assert.True(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.CommandInvoked));
        Assert.True(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.InteractionCompleted));
        Assert.True(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.ValidationRequested));
        Assert.True(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.SelectionChanged));

        Assert.False(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.HoverChanged));
        Assert.False(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.PointerMoved));
        Assert.False(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.LayoutUpdated));
        Assert.False(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.ScrollOffsetChanged));
        Assert.False(UiStateFeedbackPolicy.CanNotifyViewModel(UiStateFeedbackKind.AnimationStateChanged));
    }

    private sealed class SettingsView;
}
