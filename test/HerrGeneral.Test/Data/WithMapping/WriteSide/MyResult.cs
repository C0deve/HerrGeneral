namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record MyResult<TResult>(IEnumerable<object> Events, TResult Result);
public record MyEventHandlerResult(params IEnumerable<object> Events);