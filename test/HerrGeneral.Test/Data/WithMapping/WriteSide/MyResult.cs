namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record MyResult<TResult>(IEnumerable<object> Events, TResult Result);