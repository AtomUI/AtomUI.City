using AtomUI.City.Testing;

namespace AtomUI.City.Testing.Tests;

public sealed class RoutingTestHostTests
{
    [Fact]
    public void MatchReturnsViewModelTargetAndParameters()
    {
        var host = RoutingTestHost
            .CreateBuilder()
            .MapRoute("customer-details", "/customers/{id}", typeof(CustomerDetailsViewModel))
            .Build();

        var match = host.Match("/customers/42");

        Assert.True(match.IsMatch);
        Assert.Equal("customer-details", match.RouteName);
        Assert.Equal(typeof(CustomerDetailsViewModel), match.ViewModelType);
        Assert.Equal("42", match.Parameters["id"]);
    }

    [Fact]
    public void MatchReturnsNotFoundForUnknownPath()
    {
        var host = RoutingTestHost
            .CreateBuilder()
            .MapRoute("customer-details", "/customers/{id}", typeof(CustomerDetailsViewModel))
            .Build();

        var match = host.Match("/orders/42");

        Assert.False(match.IsMatch);
        Assert.Equal("CITY-ROUTE-NOT-FOUND", match.ErrorCode);
    }

    private sealed class CustomerDetailsViewModel;
}
