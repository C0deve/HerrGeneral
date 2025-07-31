namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record PongWithReadSideFailure : EventBase
{
    public PongWithReadSideFailure(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId)
    {
    }
}