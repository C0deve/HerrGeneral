namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record AddFriend : Change<Person>
{
    public AddFriend(Guid aggregateId) : base(aggregateId)
    {
    }
    
    public class Handler : ChangeHandler<Person,AddFriend>
    {
        public Handler(CtorParams ctorParams) : base(ctorParams)
        {
        }

        protected override Person Handle(Person aggregate, AddFriend command) => 
            aggregate.AddFriend("Adams", command.Id);
    }
}