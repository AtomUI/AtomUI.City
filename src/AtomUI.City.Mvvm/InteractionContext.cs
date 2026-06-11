namespace AtomUI.City.Mvvm;

public sealed class InteractionContext<TRequest>
{
    public InteractionContext(TRequest request)
    {
        Request = request;
    }

    public TRequest Request { get; }
}
