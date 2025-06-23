namespace HerrGeneral.Core;

internal interface IEventHandlerWrapper
{
    void Handle(UnitOfWorkId operationId, object @event, IServiceProvider serviceProvider);
}