namespace HerrGeneral;

/// <summary>
/// Result of an handled command.
/// </summary>
public record Result : Result<Unit>
{
    private Result() : base(Unit.Default)
    {
    }
    
    private Result(object error) : base(error)
    {
    }
    
    private Result(Exception error) : base(error)
    {
    }

    /// <summary>
    /// Factory for success
    /// </summary>
    public static Result Success() => new();

    /// <summary>
    /// Factory for success
    /// </summary>
    public static Result<TResult> Success<TResult>(TResult value) => Result<TResult>.Success(value);

    /// <summary>
    /// Factory for domain error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public new static Result DomainFail(object error) => new(error);

    /// <summary>
    /// Factory for panic exception
    /// </summary>
    /// <param name="panicException"></param>
    /// <returns></returns>
    public new static Result PanicFail(Exception panicException) => new(panicException);
    
    
    /// <summary>
    /// Evaluates a specified action based on the .current state
    /// </summary>
    /// <param name="onSuccess">The action to evaluate if the value is present.</param>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    /// <param name="onPanicError"></param>
    public void Match(Action onSuccess, Action<object> onDomainError, Action<Exception> onPanicError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onDomainError);
        ArgumentNullException.ThrowIfNull(onPanicError);

        if (IsSuccess)
            onSuccess();
        else if (IsDomainError)
            onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)} is null"));
        else if (IsPanicError)
            onPanicError(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)} is null"));
        else
            throw new InvalidOperationException("Invalid state");
    }
    
    /// <summary>
    /// Evaluates a specified function, based on the .current state
    /// </summary>
    /// <param name="onSuccess">The function to evaluate on success.</param>
    /// <param name="onDomainError">The function to evaluate on domain error.</param>
    /// <param name="onPanicError">The function to evaluate on panic error.</param>
    /// <returns>The result of the evaluated function.</returns>
    public TResultOut Match<TResultOut>(Func<TResultOut> onSuccess, Func<object, TResultOut> onDomainError, Func<Exception, TResultOut> onPanicError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onDomainError);
        ArgumentNullException.ThrowIfNull(onPanicError);

        return IsSuccess
            ? onSuccess()
            : IsDomainError
                ? onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)} is null"))
                : IsPanicError
                    ? onPanicError(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)} is null"))
                    : throw new InvalidOperationException("Invalid state");
    }
}