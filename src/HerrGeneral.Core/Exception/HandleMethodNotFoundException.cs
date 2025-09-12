using HerrGeneral.Core;

namespace HerrGeneral.Exception;

/// <summary>
/// No method found for handling event
/// </summary>
public class HandleMethodNotFoundException : System.Exception
{
    internal HandleMethodNotFoundException(Type tEvent, Type handlerType, string methodName) : 
        base($"No method found on {handlerType.GetFriendlyName()} to handle event of type '{tEvent.Name}'. (method name :{methodName})"){}
}