namespace HerrGeneral;

internal interface IEventHandlerWrapper
{
    Task Handle(object @event, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}