namespace HerrGeneral.Core.ReadSide;

internal interface IEventHandlerWrapper
{
    void Handle(object @event, IServiceProvider serviceProvider);
}