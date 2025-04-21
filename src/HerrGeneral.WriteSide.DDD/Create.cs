namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Command for aggregate creation
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public abstract record Create<TAggregate> : Create
    where TAggregate : IAggregate
{
    /// <summary>
    /// Constructor
    /// </summary>
    protected Create()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="executionDate"></param>
    protected Create(DateTime executionDate) : base(executionDate)
    {
    }
}