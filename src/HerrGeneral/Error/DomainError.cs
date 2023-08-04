﻿namespace HerrGeneral.Error;

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
        if (message.IsNullOrEmpty())
            throw new ArgumentNullException(nameof(message));
        
        Message = message;
    }

    /// <summary>
    /// Description of the error
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Cast to Domain exception
    /// </summary>
    /// <returns></returns>
    public DomainException ToException() => new(this);
    
}