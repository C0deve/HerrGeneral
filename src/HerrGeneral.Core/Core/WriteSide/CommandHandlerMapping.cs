using System.Reflection;

namespace HerrGeneral.Core.WriteSide;

internal record CommandHandlerMapping(
    MethodInfo MethodInfo,
    Type HandlerGenericType,
    Type ReturnValueType,
    Func<object, IEnumerable<object>> MapEvents,
    Func<object, object>? MapValue
);