using HerrGeneral.Error;

namespace HerrGeneral.WriteSide;

public class CommandResultV2 : IWithSuccess
{
    public Exception? PanicException { get; }
    public DomainError? DomainError { get; }

    public bool IsSuccess => !IsDomainError && !IsPanicError;
    private bool IsDomainError => DomainError != null;
    private bool IsPanicError => PanicException != null;

    private CommandResultV2() { }

    private CommandResultV2(DomainError error) => DomainError = error;

    private CommandResultV2(Exception panicException) => PanicException = panicException;

    public static CommandResultV2 Success { get; } = new();

    public static CommandResultV2 DomainFail(DomainError error) => new(error);

    public static CommandResultV2 PanicFail(Exception panicException) => new(panicException);

    /// <summary>
    /// Evaluates a specified function, based on whether a value is present or not.
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
                ? onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)}  is null"))
                : onPanicError(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)}  is null"));
    }

    /// <summary>
    /// Evaluates a specified action, based on whether a value is present or not.
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
            onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)}  is null"));
        else
            onPanicError(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)}  is null"));
    }

    /// <summary>
    /// Evaluates a specified action if a value is present.
    /// </summary>
    /// <param name="success">The action to evaluate if the value is present.</param>
    public void MatchSuccess(Action success)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));

        if (IsSuccess) success();
    }

    /// <summary>
    /// Evaluates a specified action if no value is present.
    /// </summary>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    public void MatchDomainError(Action<DomainError> onDomainError)
    {
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));

        if (IsDomainError) onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)}  is null"));
    }
        
    public void MatchPanicException(Action<Exception> onPanicException)
    {
        if (onPanicException == null) throw new ArgumentNullException(nameof(onPanicException));

        if (IsPanicError) onPanicException(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)}  is null"));
    }

    public static implicit operator bool(CommandResultV2 result) => result.IsSuccess;

    public override string ToString() =>
        Match(
            () => "OK",
            error => error.Message,
            exception => exception.Message
        );
}