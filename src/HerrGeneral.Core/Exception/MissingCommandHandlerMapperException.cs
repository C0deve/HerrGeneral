namespace HerrGeneral.Exception;

/// <summary>
/// No mapper found for handling command
/// </summary>
public class MissingCommandHandlerMapperException : System.Exception
{
    internal MissingCommandHandlerMapperException(Type tCommand, Type returnType) : base($"No handler mapper for command of type '{tCommand.Name}' and  return value of type '{returnType.Name}'"){}
}