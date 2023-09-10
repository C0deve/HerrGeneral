namespace HerrGeneral.Test.Data.WriteSide;

public record PongWithFailureInEventHandlerEvent : EventBase
{
    public PongWithFailureInEventHandlerEvent(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId) {}
}