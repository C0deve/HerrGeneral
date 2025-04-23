namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record ACreateCommandWithoutHandler(string Name) : Create<Person>;
public record ASecondCreateCommandWithoutHandler(string Name) : Create<Person>;