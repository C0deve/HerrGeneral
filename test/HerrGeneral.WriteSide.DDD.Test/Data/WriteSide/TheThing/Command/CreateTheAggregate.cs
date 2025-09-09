namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record CreateTheAggregate(string Name) : Create<TheAggregate>
{
    public class Handler : ICreateHandler<TheAggregate, CreateTheAggregate>
    {
        public TheAggregate Handle(CreateTheAggregate command, Guid aggregateId) => 
            new(aggregateId, command.Name, command.Id);
    }
}