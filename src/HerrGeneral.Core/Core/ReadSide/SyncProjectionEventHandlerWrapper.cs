using System.Diagnostics;
using HerrGeneral.Core.Diagnostics;

namespace HerrGeneral.Core.ReadSide;

internal interface ISyncProjectionEventHandlerWrapper
{
    void Handle(object @event, IServiceProvider serviceProvider);
}

internal class SyncProjectionEventHandlerWrapper<TEvent> : ISyncProjectionEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var collector = serviceProvider.GetService<ActivityTreeCollector>();

        foreach (var handler in serviceProvider.GetServices<IHandleSyncProjection<TEvent>>())
        {
            var handlerType = handler is IHandlerTypeProvider handlerTypeProvider
                ? handlerTypeProvider.GetHandlerType()
                : handler.GetType();

            using var activity = HerrGeneralDiagnostics.StartActivity(
                HerrGeneralDiagnostics.Activities.SyncProjectionsHandleEvent);

            activity?.SetTag(HerrGeneralDiagnostics.Tags.EventType, typeof(TEvent).ToString());
            activity?.SetTag(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString());

            var watch = Stopwatch.StartNew();
            var status = "Success";

            try
            {
                handler.Handle(@event);
                watch.Stop();
                activity?.SetStatus(ActivityStatusCode.Ok);
                collector?.RecordSyncProjection(typeof(TEvent), handlerType, watch.Elapsed, null);
            }
            catch (System.Exception ex)
            {
                watch.Stop();
                status = "Error";
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                collector?.RecordSyncProjection(typeof(TEvent), handlerType, watch.Elapsed, ex);
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
        }
    }
}
