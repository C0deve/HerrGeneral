namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record AddFriend(string FriendName, Guid AggregateId) : Change<Person>(AggregateId)
{
    public class Handler : IChangeHandler<Person, AddFriend>
    {
        public Person Handle(Person aggregate, AddFriend command) => 
            aggregate.AddFriend(command.FriendName, command.Id);
    }
}