﻿namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record ACreateCommandWithoutHandler(string Name) : Create<Person>(DateTime.Now);
public record ASecondCreateCommandWithoutHandler(string Name) : Create<Person>(DateTime.Now);