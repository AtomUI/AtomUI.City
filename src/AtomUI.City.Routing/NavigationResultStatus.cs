namespace AtomUI.City.Routing;

public enum NavigationResultStatus
{
    Success,
    Rejected,
    Redirected,
    Cancelled,
    Failed,
    NotFound,
    StaleRouteGraph,
    ContributionRevoked,
}
