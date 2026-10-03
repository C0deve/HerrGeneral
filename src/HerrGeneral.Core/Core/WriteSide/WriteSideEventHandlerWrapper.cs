using System.Collections.Immutable;
using HerrGeneral.Core.Diagnostics;
using static HerrGeneral.Core.WriteSide.EventHandlerPipeline;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    public IReadOnlyList<object> Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static ImmutableArray<object> Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();
        if (handlers is ICollection<IEventHandler<TEvent>> { Count: 0 })
            return [];

        var collector = serviceProvider.GetService<ActivityTreeCollector>();
        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();

        var builder = ImmutableArray.CreateBuilder<object>();

        // Execute each write-side handler through the pipeline (domain exception mapping + activity tracing) and collect resulting events
        foreach (var handler in handlers)
        {
            EventHandlerDelegate<TEvent> pipeline = handler.Handle;
            var result = pipeline
                .WithDomainExceptionMapping(domainExceptionMapper)
                .WithTracer(handler, collector)(@event);

            for (var i = 0; i < result.Count; i++) 
                builder.Add(result[i]);
        }

        return builder.ToImmutable();
    }
}