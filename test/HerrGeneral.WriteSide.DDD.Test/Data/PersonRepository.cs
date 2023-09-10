namespace HerrGeneral.WriteSide.DDD.Test.Data;

public class PersonRepository : IAggregateRepository<Person>
{
    private readonly Dictionary<Guid, Person> _persons = new(); 
    public bool HasSaved { get; private set; }
    public Person Get(Guid id, Guid sourceCommandId) => _persons[id];

    public void Save(Person aggregate, Guid sourceCommandId)
    {
        _persons[aggregate.Id] = aggregate;
        HasSaved = true;
    }
}