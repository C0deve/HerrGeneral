using System.Reflection;

namespace HerrGeneral.Core;

internal interface IReadSideEventHandlerMappings
{
    MethodInfo GetHandleMethod(Type evtType, Type handlerType);
}