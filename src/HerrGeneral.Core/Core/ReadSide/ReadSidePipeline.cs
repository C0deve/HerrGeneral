using System.Diagnostics;
using HerrGeneral.Core.Diagnostics;
using HerrGeneral.ReadSide;

namespace HerrGeneral.Core.ReadSide;

internal static class ReadSidePipeline
{
    public delegate void EventHandlerDelegate<in TEvent>(TEvent @event);

    public static EventHandlerDelegate<TEvent> WithReadSideHandlerLogging<TEvent>(
        this EventHandlerDelegate<TEvent> next,
        IProjectionEventHandler<TEvent> handler,
        ActivityTreeCollector? collector) =>
        @event =>
        {
            var handlerType = handler is IHandlerTypeProvider handlerTypeProvider
                ? handlerTypeProvider.GetHandlerType()
                : handler.GetType();

            using var activity = HerrGeneralDiagnostics.StartActivity(
                HerrGeneralDiagnostics.Activities.ReadSideHandleEvent);

            activity?.SetTag(HerrGeneralDiagnostics.Tags.EventType, typeof(TEvent).ToString());
            activity?.SetTag(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString());

            var watch = Stopwatch.StartNew();
            var status = "Success";

            collector?.HandleEvent(handlerType);
            try
            {
                next(@event);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (System.Exception ex)
            {
                status = "Error";
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
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