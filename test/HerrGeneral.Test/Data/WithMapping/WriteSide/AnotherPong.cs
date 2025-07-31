namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record AnotherPong : EventBase
{
    public AnotherPong(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId)
    {
    }
}