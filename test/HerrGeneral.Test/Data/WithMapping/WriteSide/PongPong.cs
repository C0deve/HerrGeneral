namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record PongPong : EventBase
{
    public PongPong(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId)
    {
    }
}