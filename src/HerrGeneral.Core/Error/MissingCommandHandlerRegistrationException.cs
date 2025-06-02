namespace HerrGeneral.Core.Error;

/// <summary>
/// Command handler registration exception
/// </summary>
public class MissingCommandHandlerRegistrationException : Exception
{
    internal MissingCommandHandlerRegistrationException(Type tCommand) : base($"No registered handler for command of type {tCommand.Name}")
    {
    }
}