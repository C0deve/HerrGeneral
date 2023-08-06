namespace HerrGeneral.Core.Error;

/// <summary>
/// Exception for read side event dispatch
/// </summary>
public class ReadSideException : Exception
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="innerException"></param>
    public ReadSideException(Exception innerException) : base(innerException.Message, innerException)
    {
    }
}