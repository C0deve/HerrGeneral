namespace HerrGeneral.Test.Data.WithHerrGeneralDependency.WriteSide;

public record PingWithDependenceOnHerrGeneral : CommandBase
{
    public class Handler(EventTracker eventTracker) : HerrGeneral.WriteSide.ICommandHandler<PingWithDependenceOnHerrGeneral, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(PingWithDependenceOnHerrGeneral command)
        {
            var pong = new Pong(command.Id, Guid.NewGuid());
            eventTracker.AddHandled(pong);
            return ([pong], Unit.Default);
        }
    }
}