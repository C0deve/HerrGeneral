namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

public record ChangeTheThing(string FriendName, Guid AggregateId) : Change<TheThing>(AggregateId)
{
    public class Handler : IChangeHandler<TheThing, ChangeTheThing>
    {
        public TheThing Handle(TheThing aggregate, ChangeTheThing command) => 
            aggregate.AddFriend(command.FriendName, command.Id);
    }
}