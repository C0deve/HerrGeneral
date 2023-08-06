using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PongWithFailureInEventHandlerEvent : EventBase
{
    public PongWithFailureInEventHandlerEvent(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId) {}
}