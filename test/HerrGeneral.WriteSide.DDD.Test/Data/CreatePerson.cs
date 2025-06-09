namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record CreatePerson(string Name, string Friend) : Create<Person>
{
    public class Handler : ICreateHandler<Person, CreatePerson>
    {
        public Person Handle(CreatePerson command, Guid aggregateId) => 
            new(aggregateId, command.Name, command.Friend, command.Id);
    }
}