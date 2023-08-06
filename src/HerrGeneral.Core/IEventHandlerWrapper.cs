namespace HerrGeneral.Core;

internal interface IEventHandlerWrapper
{
    Task Handle(object @event, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}