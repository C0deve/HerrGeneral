using HerrGeneral.Error;

namespace HerrGeneral.Test.Data.WriteSide;

public class PingError : DomainError
{
    public PingError() : base("Ping failed") { }
}