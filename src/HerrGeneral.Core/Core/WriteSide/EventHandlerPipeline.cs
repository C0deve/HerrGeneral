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

                try
                {
                    var result = next(@event);
                    watch.Stop();
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    collector?.RecordWriteSideHandler(handlerType, typeof(TEvent), watch.Elapsed, result, null);
                    return result;
                }
                catch (EventHandlerDomainException e)
                {
                    watch.Stop();
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e);
                    collector?.RecordWriteSideHandler(handlerType, typeof(TEvent), watch.Elapsed, null, e);
                    throw;
                }
                catch (EventHandlerException e)
                {
                    watch.Stop();
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e.InnerException ?? e);
                    collector?.RecordWriteSideHandler(handlerType, typeof(TEvent), watch.Elapsed, null, e.InnerException ?? e);
                    throw;
                }
                catch (System.Exception e)
                {
                    watch.Stop();
                    status = "Error";
                    activity?.SetStatus(ActivityStatusCode.Error, e.Message);
                    activity?.RecordException(e);
                    collector?.RecordWriteSideHandler(handlerType, typeof(TEvent), watch.Elapsed, null, e);
                    throw;
                }
                finally
                {
                    if (watch.IsRunning) watch.Stop();
                    HerrGeneralDiagnostics.EventsDuration.Record(watch.Elapsed.TotalMilliseconds,
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.EventType, typeof(TEvent).ToString()),
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString()),
                        new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.Status, status));
                }
            };
    }
}