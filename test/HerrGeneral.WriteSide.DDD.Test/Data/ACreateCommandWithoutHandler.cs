namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record ACreateCommandWithoutHandler(string Name, string Friend) : Create<Person>;
public record ASecondCreateCommandWithoutHandler : Create<Person>;