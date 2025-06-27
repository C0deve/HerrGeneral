using System.Reflection;
using HerrGeneral.Core.Error;
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
    : IEventHandler<TEvent>
    where TEvent : notnull
    where THandler : notnull
{
    public void Handle(TEvent evt)
    {
        var mapping = eventHandlerMappingsProvider.GetFromEventType(evt.GetType());

        var handleMethod =
            typeof(THandler).GetMethod(mapping.MethodInfo.Name) ?? throw new InvalidOperationException();

        try
        {
            var result = handleMethod.Invoke(handler, [evt]);
            if (result == null || mapping.MapEvents == null) return;
            
            try
            {
                var events = mapping.MapEvents(result);
            }
            catch (Exception e)
            {
                var mappingHandlerType = mapping.MethodInfo.DeclaringType!;
                throw new ConversionException(result.GetType(), mappingHandlerType, e);
            }

            //return (events, value);
        }
        // throw only the innerException of TargetInvocationException produce by handleMethod.Invoke. 
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }
}