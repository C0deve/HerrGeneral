namespace HerrGeneral.WriteSide.DDD.Test.Data.ReadModel;

public class ProjectionBase<T>
{
    protected readonly List<T> Data = [];
    public IReadOnlyCollection<T> All() => Data.AsReadOnly();
}