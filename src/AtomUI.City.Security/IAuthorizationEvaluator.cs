namespace AtomUI.City.Security;

public interface IAuthorizationEvaluator
{
    ValueTask<AuthorizationResult> EvaluateAsync(
        AuthorizationRequest request,
        CancellationToken cancellationToken = default);
}
