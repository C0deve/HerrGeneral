namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record Pong : EventBase
{
    public Pong(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId)
    {
    }
}