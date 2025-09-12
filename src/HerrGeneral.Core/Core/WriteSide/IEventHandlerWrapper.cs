namespace HerrGeneral.Core.WriteSide;

internal interface IEventHandlerWrapper
{
    IEnumerable<object> Handle(object @event, IServiceProvider serviceProvider);
}