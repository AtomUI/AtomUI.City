using AtomUI.City.Security;

namespace AtomUI.City.Security.Tests;

public sealed class AuthorizationResultTests
{
    [Fact]
    public void MessageArgumentsRejectExternalMutation()
    {
        var arguments = new List<object?> { "settings.write" };
        var result = AuthorizationResult.Forbidden("settings.write", messageArguments: arguments);
        var exposedArguments = Assert.IsAssignableFrom<IList<object?>>(result.MessageArguments);

        arguments[0] = "changed";

        Assert.Throws<NotSupportedException>(() => exposedArguments[0] = "changed");
        Assert.Equal("settings.write", result.MessageArguments![0]);
    }
}
