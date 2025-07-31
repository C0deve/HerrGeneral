namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record PingWithPanicException : CommandBase
{
    public class Handler : CommandHandler<PingWithPanicException>
    {
        protected override IEnumerable<object> InnerHandle(PingWithPanicException command)
            => throw new SomePanicException();
    }
}