namespace HerrGeneral.Core;

internal interface IEventHandlerWrapper
{
    void Handle(object @event, IServiceProvider serviceProvider);
}