namespace HerrGeneral.Test.Data.WriteSide;

public record MyResult<TResult>(IEnumerable<object> Events, TResult Result);