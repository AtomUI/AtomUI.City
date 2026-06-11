namespace AtomUI.City.Presentation;

public sealed class RouteOutletCommitPlan
{
    private RouteOutletCommitPlan(
        string outletName,
        RouteOutletOperation operation,
        BoundViewHandle? handle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outletName);

        OutletName = outletName;
        Operation = operation;
        Handle = handle;
    }

    public string OutletName { get; }

    public RouteOutletOperation Operation { get; }

    public BoundViewHandle? Handle { get; }

    public static RouteOutletCommitPlan Replace(
        string outletName,
        BoundViewHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);

        return new RouteOutletCommitPlan(
            outletName,
            RouteOutletOperation.Replace,
            handle);
    }

    public static RouteOutletCommitPlan Clear(string outletName)
    {
        return new RouteOutletCommitPlan(
            outletName,
            RouteOutletOperation.Clear,
            handle: null);
    }
}
