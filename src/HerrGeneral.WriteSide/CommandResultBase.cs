namespace HerrGeneral.WriteSide;

/// <summary>
/// Base class for command result implementation
/// </summary>
public abstract record CommandResultBase : IWithSuccess
{
    /// <summary>
    /// A panic exception
    /// </summary>
    internal Exception? PanicException;

    /// <summary>
    /// A domain exception
    /// </summary>
    internal DomainError? DomainError;

    /// <summary>
    /// 
    /// </summary>
    internal bool IsDomainError => DomainError != null;

    internal bool IsPanicError => PanicException != null;

    /// <summary>
    /// Operation succeeded.
    /// </summary>
    public bool IsSuccess => !IsDomainError && !IsPanicError;

    /// <summary>
    /// Ctor
    /// </summary>
    internal CommandResultBase()
    {
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="error"></param>
    internal CommandResultBase(DomainError error) => DomainError = error;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="panicException"></param>
    internal CommandResultBase(Exception panicException) => PanicException = panicException;

    /// <summary>
    /// Evaluates a specified action on domain error.
    /// </summary>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    public void MatchDomainError(Action<DomainError> onDomainError)
    {
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));

        if (IsDomainError) onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)}  is null"));
    }

    /// <summary>
    /// Evaluates a specified action on panic exception.
    /// </summary>
    /// <param name="onPanicException"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void MatchPanicException(Action<Exception> onPanicException)
    {
        if (onPanicException == null) throw new ArgumentNullException(nameof(onPanicException));

        if (IsPanicError) onPanicException(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)}  is null"));
    }
}