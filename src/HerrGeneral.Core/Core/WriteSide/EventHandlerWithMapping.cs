using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using HerrGeneral.Core.Configuration;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.Exception;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Internal handler used to map client event handler
/// </summary>
/// <param name="handler"></param>
/// <param name="eventHandlerMappingsProvider"></param>
/// <typeparam name="TEvent"></typeparam>
/// <typeparam name="THandler"></typeparam>
internal class EventHandlerWithMapping<TEvent, THandler>(THandler handler, IWriteSideEventHandlerMappings eventHandlerMappingsProvider)
    : IEventHandler<TEvent>, IHandlerTypeProvider
    where TEvent : notnull
    where THandler : notnull
{
    private static readonly ConcurrentDictionary<Type, Func<THandler, TEvent, object?>> InvokerCache = new();

    public IReadOnlyList<object> Handle(TEvent evt)
    {
        var (handleMethod, mapping) = eventHandlerMappingsProvider.GetHandleMethod(typeof(TEvent), typeof(THandler));

        var invoker = InvokerCache.GetOrAdd(evt.GetType(), _ => CompileInvoker(handleMethod));

        object? result;
        try
        {
            result = invoker(handler, evt);
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }

        if (result is null)
            return [];

        // Event resolution logic:
        // 1. If no custom mapping function is provided (MapEvents is null):
        //    - Try to cast the result directly to IReadOnlyList<object> (handler already returns events)
        //    - If cast fails, throw exception indicating missing conversion function
        // 2. If a custom mapping function is provided:
        //    - Apply the transformation function to convert the result into events
        //    - Handle any conversion errors by wrapping them in ConversionException
        switch (mapping.MapEvents)
        {
            case null when result is IReadOnlyList<object> events:
                return events;
            case null when result is IEnumerable<object> enumerableEvents:
                return enumerableEvents.ToList();
            case null:
                throw new InvalidOperationException(
                    $"Handler type '{typeof(THandler).Name}' is registered without a conversion function " +
                    $"and its return value of type '{result.GetType().Name}' cannot be converted to a collection of events. " +
                    $"Either make the handler return IReadOnlyList<object> or register it with a mapping function using " +
                    $"{nameof(Configuration.Configuration)}.{nameof(ConfigurationBuilder.RegisterWriteSideEventHandlerWithMapping)} method.");
            default:
                try
                {
                    return mapping.MapEvents(result);
                }
                catch (System.Exception e)
                {
                    var mappingHandlerType = mapping.MethodInfo.DeclaringType!;
                    throw new ConversionException(result.GetType(), mappingHandlerType, e);
                }
        }
    }

    public Type GetHandlerType() => typeof(THandler);

    private static Func<THandler, TEvent, object?> CompileInvoker(MethodInfo methodInfo)
    {
        var handlerParam = Expression.Parameter(typeof(THandler), "handler");
        var evtParam = Expression.Parameter(typeof(TEvent), "evt");

        var methodParamType = methodInfo.GetParameters()[0].ParameterType;
        Expression typedEvt = methodParamType == typeof(TEvent)
            ? evtParam
            : Expression.Convert(evtParam, methodParamType);

        var call = Expression.Call(handlerParam, methodInfo, typedEvt);
        Expression body = methodInfo.ReturnType == typeof(void)
            ? Expression.Block(call, Expression.Constant(null, typeof(object)))
            : Expression.Convert(call, typeof(object));

        return Expression.Lambda<Func<THandler, TEvent, object?>>(body, handlerParam, evtParam).Compile();
    }
}
