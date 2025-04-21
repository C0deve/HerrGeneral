using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithPanicException : Change
{
    public class Handler : ChangeHandler<PingWithPanicException>
    {
        public override (IEnumerable<object> Events, Unit Result) Handle(PingWithPanicException command, CancellationToken cancellationToken) 
            => throw new SomePanicException();
    }
}