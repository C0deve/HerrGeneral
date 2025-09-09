namespace HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;

public abstract class Projection<T>
{
    private readonly List<T> _data = [];

    public IReadOnlyCollection<T> All() => _data.AsReadOnly();

    protected void Add(T projection) => _data.Add(projection);

    protected void Update(Func<T, bool> predicate, Func<T, T> action) =>
        _data
            .Where(predicate)
            .ToList()
            .ForEach(item => 
                _data[_data.IndexOf(item)] = action(item));
}