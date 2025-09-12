namespace HerrGeneral.DDD;

/// <summary>
/// Marker interface for creation command with dynamic handler
/// </summary>
/// <typeparam name="TAggregate"></typeparam>
public interface INoHandlerCreate<TAggregate> where TAggregate : IAggregate;