namespace HerrGeneral.Core.WriteSide;

internal interface IEventHandlerWrapper
{
    IReadOnlyList<object> Handle(object @event, IServiceProvider serviceProvider);
}