namespace AtomUI.City.State;

public sealed class StateAccessDeniedException : InvalidOperationException
{
    public StateAccessDeniedException(string stateName)
        : base($"Write access to state '{stateName}' was denied.")
    {
        StateName = stateName;
    }

    public string StateName { get; }
}
