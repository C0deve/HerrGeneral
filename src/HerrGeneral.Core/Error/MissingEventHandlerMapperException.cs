namespace HerrGeneral.Core.Error;

/// <summary>
/// No mapper found for handling event
/// </summary>
public class MissingEventHandlerMapperException : Exception
{
    internal MissingEventHandlerMapperException(Type tEvent) : base($"No handler mapper for event of type '{tEvent.Name}'"){}
}