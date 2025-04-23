namespace HerrGeneral.Core;

internal interface IEventHandlerWrapper
{
    void Handle(Guid operationId, object @event, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}