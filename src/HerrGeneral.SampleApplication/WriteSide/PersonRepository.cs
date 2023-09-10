using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.WriteSide;

public class PersonRepository : IAggregateRepository<Person>
{
    private readonly Dictionary<Guid, Person> _persons = new(); 
    public Person Get(Guid id, Guid sourceCommandId) => 
        _persons[id];

    public void Save(Person aggregate, Guid sourceCommandId) => 
        _persons[aggregate.Id] = aggregate;
}