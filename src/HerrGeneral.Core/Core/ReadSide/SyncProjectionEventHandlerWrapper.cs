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

            collector?.HandleSyncProjection(handlerType);

            try
            {
                handler.Handle(@event);
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
        }
    }
}
