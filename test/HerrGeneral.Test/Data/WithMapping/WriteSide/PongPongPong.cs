namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record PongPongPong : EventBase
{
    public PongPongPong(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId)
    {
    }
}