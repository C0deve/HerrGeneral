using HerrGeneral.DDD;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record CreateTheThing(string Name) : Create<TheThing>
{
    public class Handler : ICreateHandler<TheThing, CreateTheThing>
    {
        public TheThing Handle(CreateTheThing command, Guid aggregateId) => 
            new(aggregateId, command.Name, command.Id);
    }
}