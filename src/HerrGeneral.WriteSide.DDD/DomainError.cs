namespace HerrGeneral.WriteSide.DDD;

/// <summary>
/// Error coming from the domain 
/// </summary>
public abstract class DomainError
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected DomainError(string message)
    {
        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message));
        
        Message = message;
    }

    /// <summary>
    /// Description of the error
    /// </summary>
    public string Message { get; }
}