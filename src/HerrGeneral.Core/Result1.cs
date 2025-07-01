namespace HerrGeneral.Core;

/// <summary>
/// Result of running a command
/// </summary>
public record Result<TResult>
{
    private readonly TResult? _value;
    
    /// <summary>
    /// A panic exception
    /// </summary>
    internal readonly Exception? PanicException;

    /// <summary>
    /// A domain exception
    /// </summary>
    internal readonly object? DomainError;
   
    internal bool IsPanicError => PanicException != null;

    /// <summary>
    /// Operation succeeded.
    /// </summary>
    public bool IsSuccess => !IsDomainError && !IsPanicError;
    
    /// <summary>
    /// Result has a domain error
    /// </summary>
    protected bool IsDomainError => DomainError != null;

    /// <summary>
    /// Ctor for success
    /// </summary>
    /// <param name="value"></param>
    protected Result(TResult value) => _value = value;

    /// <summary>
    /// Ctor for domain exception
    /// </summary>
    /// <param name="error"></param>
    protected Result(object error) => DomainError = error;

    /// <summary>
    /// Ctor for panic exception
    /// </summary>
    /// <param name="panicException"></param>
    protected Result(Exception panicException) => PanicException = panicException;

    /// <summary>
    /// Factory for success
    /// </summary>
    public static Result<TResult> Success(TResult value) => new(value);

    /// <summary>
    /// Factory for domain error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<TResult> DomainFail(object error) => new(error);

    /// <summary>
    /// Factory for panic exception
    /// </summary>
    /// <param name="panicException"></param>
    /// <returns></returns>
    public static Result<TResult> PanicFail(Exception panicException) => new(panicException);

    /// <summary>
    /// Evaluates a specified function, based on the .current state
    /// </summary>
    /// <param name="onSuccess">The function to evaluate on success.</param>
    /// <param name="onDomainError">The function to evaluate on domain error.</param>
    /// <param name="onPanicError">The function to evaluate on panic error.</param>
    /// <returns>The result of the evaluated function.</returns>
    public TResultOut Match<TResultOut>(Func<TResult, TResultOut> onSuccess, Func<object, TResultOut> onDomainError, Func<Exception, TResultOut> onPanicError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onDomainError);
        ArgumentNullException.ThrowIfNull(onPanicError);

        return IsSuccess
            ? onSuccess(_value ?? throw new InvalidOperationException($"{nameof(_value)} is null"))
            : IsDomainError
                ? onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)} is null"))
                : IsPanicError
                    ? onPanicError(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)} is null"))
                    : throw new InvalidOperationException("Invalid state");
    }

    /// <summary>
    /// Evaluates a specified action based on the .current state
    /// </summary>
    /// <param name="onSuccess">The action to evaluate if the value is present.</param>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    /// <param name="onPanicError"></param>
    public void Match(Action<TResult> onSuccess, Action<object> onDomainError, Action<Exception> onPanicError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onDomainError);
        ArgumentNullException.ThrowIfNull(onPanicError);

        if (IsSuccess)
            onSuccess(_value ?? throw new InvalidOperationException($"{nameof(_value)} is null"));
        else if (IsDomainError)
            onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)} is null"));
        else if (IsPanicError)
            onPanicError(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)} is null"));
        else
            throw new InvalidOperationException("Invalid state");
    }

    /// <summary>
    /// Evaluates a specified action based on the .current state
    /// </summary>
    /// <param name="success">The action to evaluate if the value is present.</param>
    public void MatchSuccess(Action<TResult> success)
    {
        ArgumentNullException.ThrowIfNull(success);

        if (IsSuccess) success(_value ?? throw new InvalidOperationException($"{nameof(_value)} is null"));
    }

    /// <summary>
    /// Evaluates a specified action on domain error.
    /// </summary>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    public void MatchDomainError(Action<object> onDomainError)
    {
        ArgumentNullException.ThrowIfNull(onDomainError);

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
        ArgumentNullException.ThrowIfNull(onPanicException);

        if (IsPanicError) onPanicException(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)}  is null"));
    }

    /// <summary>
    /// True if success, else false.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator bool(Result<TResult> result) => result.IsSuccess;

    /// <summary>
    /// Display the result
    /// </summary>
    /// <returns></returns>
    public override string ToString() =>
        Match(
            id => $"Result<{id}>",
            error => error.ToString(),
            exception => exception.Message
        ) ?? string.Empty;
}