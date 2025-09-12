namespace HerrGeneral.Exception;

/// <summary>
/// Command handler registration exception
/// </summary>
public class MissingCommandHandlerRegistrationException : System.Exception
{
    internal MissingCommandHandlerRegistrationException(Type tCommand) : base($"No registered handler for command of type {tCommand.Name}")
    {
    }
}