namespace HerrGeneral.Core;

internal interface IWriteSideEventHandlerMappings
{
    EventHandlerMapping GetFromEvent(object evt);
}