namespace AtomUI.City.Security;

public enum SecurityFailureKind
{
    None,
    AuthenticationRequired,
    AuthenticationExpired,
    Forbidden,
    PolicyNotFound,
    PermissionNotFound,
    RequirementFailed,
    EvaluatorFailed,
    ContributionRevoked,
    CapabilityDenied,
    Cancelled,
}
