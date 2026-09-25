using System.Diagnostics;
using HerrGeneral.Core.Diagnostics;

namespace HerrGeneral.Core.ReadSide;

internal interface IPostTransactionEventHandlerWrapper
{
    void Handle(object @event, IServiceProvider serviceProvider);
}

internal class PostTransactionEventHandlerWrapper<TEvent> : IPostTransactionEventHandlerWrapper
{
    public void Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static void Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var collector = serviceProvider.GetService<ActivityTreeCollector>();

        // 1. Post-transaction Projections (IHandlePostProjection and IProjectionEventHandler)
        foreach (var handler in serviceProvider.GetServices<IHandlePostProjection<TEvent>>())
        {
            var handlerType = handler is IHandlerTypeProvider handlerTypeProvider
                ? handlerTypeProvider.GetHandlerType()
                : handler.GetType();

            using var activity = HerrGeneralDiagnostics.StartActivity(
                HerrGeneralDiagnostics.Activities.PostTransactionHandleEvent);

            activity?.SetTag(HerrGeneralDiagnostics.Tags.EventType, typeof(TEvent).ToString());
            activity?.SetTag(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString());
            activity?.SetTag(HerrGeneralDiagnostics.Tags.PostTransactionKind, "post-projection");

            var watch = Stopwatch.StartNew();
            var status = "Success";

            try
            {
                handler.Handle(@event);
                watch.Stop();
                activity?.SetStatus(ActivityStatusCode.Ok);
                collector?.RecordPostTransaction(typeof(TEvent), handlerType, "post-projection", watch.Elapsed, null);
            }
            catch (System.Exception ex)
            {
                watch.Stop();
                status = "Error";
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                collector?.RecordPostTransaction(typeof(TEvent), handlerType, "post-projection", watch.Elapsed, ex);
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

        // 2. Post-transaction Side Effects (IHandleSideEffect)
        foreach (var handler in serviceProvider.GetServices<IHandleSideEffect<TEvent>>())
        {
            var handlerType = handler is IHandlerTypeProvider handlerTypeProvider
                ? handlerTypeProvider.GetHandlerType()
                : handler.GetType();

            using var activity = HerrGeneralDiagnostics.StartActivity(
                HerrGeneralDiagnostics.Activities.PostTransactionHandleEvent);

            activity?.SetTag(HerrGeneralDiagnostics.Tags.EventType, typeof(TEvent).ToString());
            activity?.SetTag(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString());
            activity?.SetTag(HerrGeneralDiagnostics.Tags.PostTransactionKind, "side-effect");

            var watch = Stopwatch.StartNew();
            var status = "Success";

            try
            {
                handler.Handle(@event);
                watch.Stop();
                activity?.SetStatus(ActivityStatusCode.Ok);
                collector?.RecordPostTransaction(typeof(TEvent), handlerType, "side-effect", watch.Elapsed, null);
            }
            catch (System.Exception ex)
            {
                watch.Stop();
                status = "Error";
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                collector?.RecordPostTransaction(typeof(TEvent), handlerType, "side-effect", watch.Elapsed, ex);
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
