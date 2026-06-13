using AtomUI.City.Presentation;

namespace AtomUI.City.Presentation.Tests;

public sealed class PresentationTextDescriptorTests
{
    [Fact]
    public void NotificationMessageArgumentsRejectExternalMutation()
    {
        var arguments = new List<object?> { 12 };
        var descriptor = new NotificationTextDescriptor(
            "sync-completed",
            messageKey: "Notifications.Sync.Message",
            messageArguments: arguments);
        var exposedArguments = Assert.IsAssignableFrom<IList<object?>>(descriptor.MessageArguments);

        arguments[0] = 99;

        Assert.Throws<NotSupportedException>(() => exposedArguments[0] = 99);
        Assert.Equal(12, descriptor.MessageArguments![0]);
    }

    [Fact]
    public void ErrorMessageArgumentsRejectExternalMutation()
    {
        var arguments = new List<object?> { "GET /orders" };
        var descriptor = new ErrorMessageDescriptor(
            "data-timeout",
            messageKey: "Errors.Data.Timeout",
            messageArguments: arguments);
        var exposedArguments = Assert.IsAssignableFrom<IList<object?>>(descriptor.MessageArguments);

        arguments[0] = "Changed";

        Assert.Throws<NotSupportedException>(() => exposedArguments[0] = "Changed");
        Assert.Equal("GET /orders", descriptor.MessageArguments![0]);
    }
}
