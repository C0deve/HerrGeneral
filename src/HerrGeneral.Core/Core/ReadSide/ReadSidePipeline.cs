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

            try
            {
                next(@event);
                watch.Stop();
                activity?.SetStatus(ActivityStatusCode.Ok);
                collector?.RecordReadSideHandler(typeof(TEvent), handlerType, watch.Elapsed, null);
            }
            catch (System.Exception ex)
            {
                watch.Stop();
                status = "Error";
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                collector?.RecordReadSideHandler(typeof(TEvent), handlerType, watch.Elapsed, ex);
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