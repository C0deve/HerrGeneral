using System.Reflection;

namespace HerrGeneral.Core.Configuration;

internal record EventHandlerMapping(
    MethodInfo MethodInfo,
    Type HandlerGenericType,
    Func<object, IReadOnlyList<object>>? MapEvents
);