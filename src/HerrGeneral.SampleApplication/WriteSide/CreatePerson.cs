using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.WriteSide;

public record CreatePerson : CreateAggregate<Person>
{
    private readonly string _myFriend;
    private readonly string _name;

    public CreatePerson(string name, string myFriend)
    {
        _myFriend = myFriend;
        _name = name;
    }

    public class Handler: CreateAggregateHandler<Person,CreatePerson>
    {
        public Handler(CtorParams @params) : base(@params)
        {
        }

        protected override Person Handle(CreatePerson command, Guid aggregateId) => 
            new(aggregateId, command._name, command._myFriend, command.Id);
    }
}