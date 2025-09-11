namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Marker interface for change command with dynamic handler
/// </summary>
public interface INoHandlerChange<TAggregate> where TAggregate : IAggregate;