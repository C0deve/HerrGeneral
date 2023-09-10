namespace HerrGeneral.Test.Data.WriteSide;

public record PongWithReadSideFailure : EventBase
{
    public PongWithReadSideFailure(Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId)
    {
    }
}