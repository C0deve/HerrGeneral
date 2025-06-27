using System.Reflection;

namespace HerrGeneral.Core;

internal record EventHandlerMapping(
    MethodInfo MethodInfo,
    Type HandlerGenericType,
    Func<object, IEnumerable<object>>? MapEvents
);