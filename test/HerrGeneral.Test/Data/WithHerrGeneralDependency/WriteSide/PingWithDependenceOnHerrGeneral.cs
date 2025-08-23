using HerrGeneral.Core;
using HerrGeneral.Test.Data.WithMapping.WriteSide;

namespace HerrGeneral.Test.Data.WithHerrGeneralDependency.WriteSide;

public record PingWithDependenceOnHerrGeneral : CommandBase
{
    public class Handler(EventTracker eventTracker) : HerrGeneral.WriteSide.ICommandHandler<PingWithDependenceOnHerrGeneral, Unit>
    {
        public (IEnumerable<object> Events, Unit Result) Handle(PingWithDependenceOnHerrGeneral command)
        {
            var pong = new Pong(command.Id, Guid.NewGuid());
            eventTracker.AddHandled(pong);
            return ([pong], Unit.Default);
        }
    }
}