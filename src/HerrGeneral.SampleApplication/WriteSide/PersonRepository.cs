using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.WriteSide;

public class PersonRepository : IAggregateRepository<Person>
{
    private readonly Dictionary<Guid, Person> _persons = new(); 
    public Person Get(Guid id) => 
        _persons[id];

    public void Save(Person aggregate) => 
        _persons[aggregate.Id] = aggregate;
}