namespace HerrGeneral.Core;

internal interface IWriteSideEventHandlerMappings
{
    EventHandlerMapping GetFromEventType(Type evtType);
}