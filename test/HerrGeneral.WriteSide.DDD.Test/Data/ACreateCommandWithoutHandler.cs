namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record ACreateCommandWithoutHandler(string Name) : CreateAggregate<Person>(DateTime.Now);