namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public class MyDomainException(string message) : System.Exception(message);
public class PingError() : MyDomainException("Ping failed");