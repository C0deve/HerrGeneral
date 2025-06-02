namespace HerrGeneral.Core.Error;

/// <summary>
/// No mapper found for handling command
/// </summary>
public class MissingCommandHandlerMapperException : Exception
{
    internal MissingCommandHandlerMapperException(Type tCommand, Type returnType) : base($"No handler mapper for command of type '{tCommand.Name}' and  return value of type '{returnType.Name}'"){}
}