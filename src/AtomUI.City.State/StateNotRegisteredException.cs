namespace AtomUI.City.State;

public sealed class StateNotRegisteredException : KeyNotFoundException
{
    public StateNotRegisteredException(string stateName)
        : base($"State '{stateName}' is not registered.")
    {
        StateName = stateName;
    }

    public string StateName { get; }
}
