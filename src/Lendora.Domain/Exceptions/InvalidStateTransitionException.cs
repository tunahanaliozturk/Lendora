namespace Lendora.Domain.Exceptions;

/// <summary>
/// Thrown when an aggregate attempts a state transition that is not permitted
/// by its configured state machine.
/// </summary>
public class InvalidStateTransitionException : DomainException
{
    public string CurrentState { get; }
    public string Trigger { get; }

    public InvalidStateTransitionException(string currentState, string trigger)
        : base($"Invalid state transition: cannot fire trigger '{trigger}' from state '{currentState}'.")
    {
        CurrentState = currentState;
        Trigger = trigger;
    }

    public InvalidStateTransitionException(string currentState, string trigger, Exception innerException)
        : base($"Invalid state transition: cannot fire trigger '{trigger}' from state '{currentState}'.", innerException)
    {
        CurrentState = currentState;
        Trigger = trigger;
    }
}
