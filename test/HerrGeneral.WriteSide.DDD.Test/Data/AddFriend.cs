namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record AddFriend(Guid AggregateId) : Change<Person>(AggregateId)
{
    public class Handler(ChangeHandler<Person, AddFriend>.CtorParams ctorParams) : ChangeHandler<Person, AddFriend>(ctorParams)
    {
        protected override Person Handle(Person aggregate, AddFriend command) => 
            aggregate.AddFriend("Adams", command.Id);
    }
}