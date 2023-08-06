using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PingError : DomainError
{
    public PingError() : base("Ping failed") { }
}