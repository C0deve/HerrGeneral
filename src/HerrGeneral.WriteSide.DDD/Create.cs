namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Command for aggregate creation
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public abstract record Create<TAggregate> : CommandBase
    where TAggregate : IAggregate;