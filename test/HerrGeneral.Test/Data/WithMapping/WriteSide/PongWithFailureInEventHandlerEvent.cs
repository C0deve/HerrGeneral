namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record PongWithFailureInEventHandlerEvent : EventBase
{
    public PongWithFailureInEventHandlerEvent(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId) {}
}