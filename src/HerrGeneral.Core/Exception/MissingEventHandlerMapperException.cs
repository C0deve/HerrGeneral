namespace HerrGeneral.Exception;

/// <summary>
/// No mapper found for handling event
/// </summary>
public class MissingEventHandlerMapperException : System.Exception
{
    internal MissingEventHandlerMapperException(Type tEvent) : base($"No handler mapper for event of type '{tEvent.Name}'"){}
}