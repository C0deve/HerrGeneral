using System.Collections.Immutable;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.WriteSide;

internal class WriteSideEventHandlerWrapper<TEvent> : IEventHandlerWrapper
{
    public IReadOnlyList<object> Handle(object @event, IServiceProvider serviceProvider) =>
        Handle((TEvent)@event, serviceProvider);

    private static ImmutableArray<object> Handle(TEvent @event, IServiceProvider serviceProvider)
    {
        var tracer = serviceProvider.GetService<CommandExecutionTracer>();
        var domainExceptionMapper = serviceProvider.GetRequiredService<DomainExceptionMapper>();

        return serviceProvider
            .GetServices<IEventHandler<TEvent>>()
            .Select(handler => Start(handler)
                .WithDomainExceptionMapping(domainExceptionMapper)
                .WithTracer(handler, tracer))
            .SelectMany(pipeline => pipeline(@event))
            // ReSharper disable once UseCollectionExpression
            .ToImmutableArray();
    }

    private static EventHandlerPipeline.EventHandlerDelegate<TEvent> Start(IEventHandler<TEvent> eventHandler) =>
        eventHandler.Handle;
}