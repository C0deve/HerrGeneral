namespace HerrGeneral.WriteSide;

/// <summary>
/// Result of an handled command.
/// </summary>
public sealed class CommandResultV2 : IWithSuccess
{
    private readonly Exception? _panicException;
    private readonly DomainError? _domainError;
    private bool IsDomainError => _domainError != null;
    private bool IsPanicError => _panicException != null;
    
    /// <summary>
    /// Operation succeeded.
    /// </summary>
    public bool IsSuccess => !IsDomainError && !IsPanicError;
    
    private CommandResultV2() { }

    private CommandResultV2(DomainError error) => _domainError = error;

    private CommandResultV2(Exception panicException) => _panicException = panicException;

    /// <summary>
    /// Factory for success.
    /// </summary>
    public static CommandResultV2 Success { get; } = new();

    /// <summary>
    /// Factory for domain error.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static CommandResultV2 DomainFail(DomainError error) => new(error);

    /// <summary>
    /// Factory for panic exception.
    /// </summary>
    /// <param name="panicException"></param>
    /// <returns></returns>
    public static CommandResultV2 PanicFail(Exception panicException) => new(panicException);

    /// <summary>
    /// Evaluates a specified function, based on the .current state.
    /// </summary>
    /// <param name="onSuccess">The function to evaluate on success.</param>
    /// <param name="onDomainError">The function to evaluate on domain error.</param>
    /// <param name="onPanicError">The function to evaluate on panic error.</param>
    /// <returns>The result of the evaluated function.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<DomainError, TResult> onDomainError, Func<Exception, TResult> onPanicError)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));
        if (onPanicError == null) throw new ArgumentNullException(nameof(onPanicError));
         
        return IsSuccess
            ? onSuccess()
            : IsDomainError
                ? onDomainError(_domainError ?? throw new InvalidOperationException($"{nameof(_domainError)} is null"))
                : onPanicError(_panicException ?? throw new InvalidOperationException($"{nameof(_panicException)} is null"));
    }

    /// <summary>
    /// Evaluates a specified action, based on the .current state.
    /// </summary>
    /// <param name="onSuccess">The action to evaluate if the value is present.</param>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    /// <param name="onPanicError"></param>
    public void Match(Action onSuccess, Action<DomainError> onDomainError, Action<Exception> onPanicError)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));

        if (IsSuccess)
            onSuccess();
        else if (IsDomainError)
            onDomainError(_domainError ?? throw new InvalidOperationException($"{nameof(_domainError)} is null"));
        else
            onPanicError(_panicException ?? throw new InvalidOperationException($"{nameof(_panicException)} is null"));
    }

    /// <summary>
    /// Evaluates a specified action on success.
    /// </summary>
    /// <param name="success">The action to evaluate if the value is present.</param>
    public void MatchSuccess(Action success)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));

        if (IsSuccess) success();
    }

    /// <summary>
    /// Evaluates a specified action on domain error.
    /// </summary>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    public void MatchDomainError(Action<DomainError> onDomainError)
    {
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));

        if (IsDomainError) onDomainError(_domainError ?? throw new InvalidOperationException($"{nameof(_domainError)}  is null"));
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

        if (IsPanicError) onPanicException(_panicException ?? throw new InvalidOperationException($"{nameof(_panicException)}  is null"));
    }

    /// <summary>
    /// True if success, else false.
    /// </summary>
    /// <param name="resultV2"></param>
    /// <returns></returns>
    public static implicit operator bool(CommandResultV2 resultV2) => resultV2.IsSuccess;

    /// <summary>
    /// Display the result
    /// </summary>
    /// <returns></returns>
    public override string ToString() =>
        Match(
            () => "OK",
            error => error.Message,
            exception => exception.Message
        );
}