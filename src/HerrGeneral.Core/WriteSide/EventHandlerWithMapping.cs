using System.Reflection;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.Registration;
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
    public IEnumerable<object> Handle(TEvent evt)
    {
        var (handleMethod, mapping) = eventHandlerMappingsProvider.GetHandleMethod(typeof(TEvent), typeof(THandler));

        try
        {
            var result = handleMethod.Invoke(handler, [evt]);
            if (result == null)
                 return [];
            
            // Event resolution logic:
            // 1. If no custom mapping function is provided (MapEvents is null):
            //    - Try to cast the result directly to IEnumerable<object> (handler already returns events)
            //    - If cast fails, throw exception indicating missing conversion function
            // 2. If a custom mapping function is provided:
            //    - Apply the transformation function to convert the result into events
            //    - Handle any conversion errors by wrapping them in ConversionException
            switch (mapping.MapEvents)
            {
                case null when result is IEnumerable<object> events:
                    return events;
                case null:
                    throw new InvalidOperationException(
                        $"Handler type '{typeof(THandler).Name}' is registered without a conversion function " +
                        $"and its return value of type '{result.GetType().Name}' cannot be converted to a collection of events. " +
                        $"Either make the handler return IEnumerable<object> or register it with a mapping function using " +
                        $"{nameof(Configuration)}.{nameof(Configuration.RegisterWriteSideEventHandlerWithMapping)} method.");
                default:
                    try
                    {
                        return mapping.MapEvents(result);
                    }
                    catch (Exception e)
                    {
                        var mappingHandlerType = mapping.MethodInfo.DeclaringType!;
                        throw new ConversionException(result.GetType(), mappingHandlerType, e);
                    }
            }
        }
        // throw only the innerException of TargetInvocationException produce by handleMethod.Invoke. 
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }

    public Type GetHandlerType() => typeof(THandler);
}