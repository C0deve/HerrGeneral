using System.Diagnostics;
using HerrGeneral.Core.Diagnostics;
using HerrGeneral.Core.ReadSide;

namespace HerrGeneral.Core.WriteSide;

internal static class EventHandlerPipeline
{
    public delegate IReadOnlyList<object> EventHandlerDelegate<in TEvent>(TEvent @event);

    extension<TEvent>(EventHandlerDelegate<TEvent> next)
    {
        public EventHandlerDelegate<TEvent> WithDomainExceptionMapping(DomainExceptionMapper mapper) =>
            @event =>
            {
                try
                {
                    return next(@event);
                }
                catch (System.Exception e)
                {
                    throw mapper.Map(e,
                        exception => new EventHandlerDomainException(exception),
                        exception => new EventHandlerException(exception));
                }
            };

        public EventHandlerDelegate<TEvent> WithTracer(IEventHandler<TEvent> handler, ActivityTreeCollector? collector) =>
            @event =>
            {
                var handlerType = handler is IHandlerTypeProvider handlerTypeProvider
                    ? handlerTypeProvider.GetHandlerType()
                    : handler.GetType();

                using var activity = HerrGeneralDiagnostics.StartActivity(
                    HerrGeneralDiagnostics.Activities.WriteSideHandleEvent);

                activity?.SetTag(HerrGeneralDiagnostics.Tags.EventType, typeof(TEvent).ToString());
                activity?.SetTag(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString());

                var watch = Stopwatch.StartNew();
                var status = "Success";

                collector?.HandleEvent(handlerType);

                try
                {
                    var result = next(@event);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return result;
                }
                catch (EventHandlerDomainException e)
                {
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e);
                    collector?.OnException(e, 2);
                    throw;
                }
                catch (EventHandlerException e)
                {
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e.InnerException ?? e);
                    collector?.OnException(e.InnerException!, 2);
                    throw;
                }
                catch (System.Exception e)
                {
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e);
                    collector?.OnException(e, 2);
                    throw;
                }
                finally
                {
                    watch.Stop();
                    HerrGeneralDiagnostics.EventsDuration.Record(watch.Elapsed.TotalMilliseconds,
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.EventType, typeof(TEvent).ToString()),
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString()),
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.Status, status));
                }
            };
    }
}