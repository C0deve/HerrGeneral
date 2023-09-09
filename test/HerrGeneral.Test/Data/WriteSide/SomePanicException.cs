namespace HerrGeneral.Test.Data.WriteSide;

public class SomePanicException : Exception
{
    public SomePanicException() : base("Houston we have a problem...")
    {
    }
}