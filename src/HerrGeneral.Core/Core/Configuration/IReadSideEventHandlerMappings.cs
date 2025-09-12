using System.Reflection;

namespace HerrGeneral.Core.Configuration;

internal interface IReadSideEventHandlerMappings
{
    MethodInfo GetHandleMethod(Type evtType, Type handlerType);
}