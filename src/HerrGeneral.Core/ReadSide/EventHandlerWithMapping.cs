using System.Reflection;
using HerrGeneral.ReadSide;

namespace HerrGeneral.Core.ReadSide;

/// <summary>
/// Internal handler used to map client event handler on read side
/// </summary>
/// <param name="handler"></param>
/// <param name="eventHandlerMappingProvider"></param>
/// <typeparam name="TEvent"></typeparam>
/// <typeparam name="THandler"></typeparam>
internal class EventHandlerWithMapping<TEvent, THandler>(THandler handler, IReadSideEventHandlerMappings eventHandlerMappingProvider)
    : IEventHandler<TEvent>, IHandlerTypeProvider
    where TEvent : notnull
    where THandler : notnull
{
    public void Handle(TEvent evt)
    {
        var handleMethod = eventHandlerMappingProvider.GetHandleMethod(typeof(TEvent), typeof(THandler));
        
        try
        {
            handleMethod.Invoke(handler, [evt]);
        }
        // throw only the innerException of TargetInvocationException produce by handleMethod.Invoke. 
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }

    public Type GetHandlerType() => typeof(THandler);
}