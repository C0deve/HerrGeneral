namespace HerrGeneral.Core;

internal interface IReadSideEventHandlerMappings
{
    EventHandlerMapping GetFromEvent(object evt);
}