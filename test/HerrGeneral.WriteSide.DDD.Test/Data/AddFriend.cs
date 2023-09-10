namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class AddFriend : ChangeAggregate<Person>
{
    public AddFriend(Guid aggregateId) : base(aggregateId)
    {
    }
    
    public class Handler : ChangeAggregateHandler<Person,AddFriend>
    {
        public Handler(CtorParams ctorParams) : base(ctorParams)
        {
        }

        protected override Person Handle(Person aggregate, AddFriend command) => 
            aggregate.AddFriend("Adams", command.Id);
    }
}