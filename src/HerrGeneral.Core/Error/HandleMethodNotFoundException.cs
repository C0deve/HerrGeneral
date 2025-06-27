namespace HerrGeneral.Core.Error;

/// <summary>
/// No method found for handling event
/// </summary>
public class HandleMethodNotFoundException : Exception
{
    internal HandleMethodNotFoundException(Type tEvent, Type handlerType, string methodName) : 
        base($"No method found on {handlerType.GetFriendlyName()} to handle event of type '{tEvent.Name}'. (method name :{methodName})"){}
}