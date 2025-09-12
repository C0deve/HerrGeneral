using System.Reflection;

namespace HerrGeneral.Core.Configuration;

internal interface IWriteSideEventHandlerMappings
{
    (MethodInfo Method, EventHandlerMapping Mapping) GetHandleMethod(Type evtType, Type handlerType);
    
}