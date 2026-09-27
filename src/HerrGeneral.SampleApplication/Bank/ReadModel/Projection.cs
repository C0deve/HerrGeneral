namespace HerrGeneral.SampleApplication.Bank.ReadModel;

public abstract class Projection<T>
{
    private readonly List<T> _data = [];
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    public IReadOnlyCollection<T> All()
    {
        lock (_lock)
        {
            return _data.ToList().AsReadOnly();
        }
    }

    protected void Add(T projection)
    {
        lock (_lock)
        {
            _data.Add(projection);
        }
    }

    protected void Update(Func<T, bool> predicate, Func<T, T> action)
    {
        lock (_lock)
        {
            _data
                .Where(predicate)
                .ToList()
                .ForEach(item => 
                    _data[_data.IndexOf(item)] = action(item));
        }
    }
}