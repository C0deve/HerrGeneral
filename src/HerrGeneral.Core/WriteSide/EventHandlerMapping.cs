using System.Reflection;

namespace HerrGeneral.Core.WriteSide;

internal record EventHandlerMapping(
    MethodInfo MethodInfo,
    Type HandlerGenericType,
    Func<object, IEnumerable<object>>? MapEvents
);