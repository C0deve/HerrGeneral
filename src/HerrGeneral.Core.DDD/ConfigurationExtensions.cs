using HerrGeneral.Core.Registration;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.Core.DDD;

/// <summary>
/// Provide extension methods for <see cref="Configuration"/>
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Map all <see cref="Create{TAggregate}"/> and <see cref="Change{TAggregate}"/> command handlers.
    /// <c>TAggregate</c> is needed to resolve the generic handler type so it doesn't matter which one you provide.
    /// </summary>
    /// <param name="config"></param>
    /// <typeparam name="TAggregate"> One of your aggregate type. (It doesn't matter which one you choose)</typeparam>
    /// <returns></returns>
    public static Configuration MapAllDDDHandlers<TAggregate>(this Configuration config) where TAggregate : Aggregate<TAggregate> =>
        config
            .MapHandler<Create<TAggregate>, CreateHandler<TAggregate, Create<TAggregate>>, (IEnumerable<object> Events, Guid Id), Guid>(
                tuple => tuple.Events,
                tuple => tuple.Id)
            .MapHandler<Change<TAggregate>, ChangeHandler<TAggregate, Change<TAggregate>>, IEnumerable<object>>(events => events);
}