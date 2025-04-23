using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithPanicException
{
    public class Handler : CommandHandler<PingWithPanicException>
    {
        protected override IEnumerable<object> Handle(PingWithPanicException command) 
            => throw new SomePanicException();
    }
}