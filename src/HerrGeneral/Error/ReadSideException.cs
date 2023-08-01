namespace HerrGeneral.Error;

public class ReadSideException : Exception
{
    public ReadSideException(Exception innerException) : base(innerException.Message, innerException)
    {
    }
}