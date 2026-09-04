using HerrGeneral.Core.Configuration;
using HerrGeneral.ReadSide;

namespace HerrGeneral.Core.ReadSide;

/// <summary>
/// Internal handler used to map client event handler on read side
/// </summary>
/// <param name="handler"></param>
/// <param name="eventHandlerMappingProvider"></param>
/// <typeparam name="TEvent"></typeparam>
/// <typeparam name="THandler"></typeparam>
internal class ProjectionEventHandlerWithMapping<TEvent, THandler>(THandler handler, IReadSideEventHandlerMappings eventHandlerMappingProvider)
    : IProjectionEventHandler<TEvent>, IHandlerTypeProvider
    where TEvent : notnull
    where THandler : notnull
{
    private static readonly ConcurrentDictionary<Type, Action<THandler, TEvent>> InvokerCache = new();

    public void Handle(TEvent evt)
    {
        var invoker = InvokerCache.GetOrAdd(evt.GetType(), _ =>
        {
            var handleMethod = eventHandlerMappingProvider.GetHandleMethod(typeof(TEvent), typeof(THandler));
            return CompileInvoker(handleMethod);
        });
        
        try
        {
            invoker(handler, evt);
        }
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }

    public Type GetHandlerType() => typeof(THandler);

    private static Action<THandler, TEvent> CompileInvoker(MethodInfo methodInfo)
    {
        var handlerParam = Expression.Parameter(typeof(THandler), "handler");
        var evtParam = Expression.Parameter(typeof(TEvent), "evt");

        var methodParamType = methodInfo.GetParameters()[0].ParameterType;
        Expression typedEvt = methodParamType == typeof(TEvent)
            ? evtParam
            : Expression.Convert(evtParam, methodParamType);

        var call = Expression.Call(handlerParam, methodInfo, typedEvt);
        return Expression.Lambda<Action<THandler, TEvent>>(call, handlerParam, evtParam).Compile();
    }
}
