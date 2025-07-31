using HerrGeneral.Core;
using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithHerrGeneralDependency.WriteSide;

public record PingWithDependenceOnHerrGeneral : CommandBase
{
    public class Handler : HerrGeneral.WriteSide.ICommandHandler<PingWithDependenceOnHerrGeneral, Unit>
    {
        public (IEnumerable<object> Events, Unit Result) Handle(PingWithDependenceOnHerrGeneral command) =>
            ([
                    new Pong(command.Id,
                        Guid.NewGuid())
                ],
                Unit.Default);
    }
}