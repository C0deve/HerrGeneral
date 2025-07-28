using HerrGeneral.Core;

namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithDependenceOnHerrGeneral
{
    public class Handler : HerrGeneral.WriteSide.ICommandHandler<PingWithDependenceOnHerrGeneral, Unit>
    {
        public (IEnumerable<object> Events, Unit Result) Handle(PingWithDependenceOnHerrGeneral command) =>
            ([
                    new Pong(
                        "pong received",
                        Guid.NewGuid(),
                        Guid.NewGuid())
                ],
                Unit.Default);
    }
}