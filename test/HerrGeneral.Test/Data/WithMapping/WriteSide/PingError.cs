namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public class MyDomainException(string message) : Exception(message);
public class PingError() : MyDomainException("Ping failed");