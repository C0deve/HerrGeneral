namespace HerrGeneral.Error;

public abstract class DomainError
{
    protected DomainError(string message)
    {
        if (message.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(message));
        
        Message = message;
    }

    public string Message { get; }

    public DomainException ToException() => new(this);
    
}