using System.Text;
using HerrGeneral.WriteSide.DDD.Utils;

namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Command for aggregate creation
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public abstract record CreateAggregate<TAggregate> : Create
    where TAggregate : IAggregate
{
    /// <summary>
    /// Constructor
    /// </summary>
    protected CreateAggregate()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="executionDate"></param>
    protected CreateAggregate(DateTime executionDate) : base(executionDate)
    {
    }
}