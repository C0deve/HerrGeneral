namespace HerrGeneral.Test.Data.WriteSide;

public class PanicException : Exception
{
    public PanicException() : base("houston we have a problem...")
    {
    }
}