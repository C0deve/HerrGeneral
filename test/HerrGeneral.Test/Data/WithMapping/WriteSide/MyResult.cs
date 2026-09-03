namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record MyResult<TResult>(IReadOnlyList<object> Events, TResult Result);
public record MyEventHandlerResult(params IReadOnlyList<object> Events);