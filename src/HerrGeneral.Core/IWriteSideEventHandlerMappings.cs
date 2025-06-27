using System.Reflection;

namespace HerrGeneral.Core;

internal interface IWriteSideEventHandlerMappings
{
    (MethodInfo Method, EventHandlerMapping Mapping) GetHandleMethod(Type evtType, Type handlerType);
    
}