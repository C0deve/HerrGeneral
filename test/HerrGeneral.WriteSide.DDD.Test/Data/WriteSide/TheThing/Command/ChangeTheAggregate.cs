namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record ChangeTheAggregate(string FriendName, Guid AggregateId) : Change<TheAggregate>(AggregateId)
{
    public class Handler : IChangeHandler<TheAggregate, ChangeTheAggregate>
    {
        public TheAggregate Handle(TheAggregate aggregate, ChangeTheAggregate command) => 
            aggregate.AddFriend(command.FriendName, command.Id);
    }
}