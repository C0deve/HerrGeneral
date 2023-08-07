namespace HerrGeneral.WriteSide;

/// <summary>
/// Result of an handled command.
/// </summary>
public sealed record CommandResult : CommandResultBase
{
    private CommandResult()
    {
    }

    private CommandResult(DomainError error) : base(error)
    {
    }

    private CommandResult(Exception panicException) : base(panicException)
    {
    }


    /// <summary>
    /// Factory for success.
    /// </summary>
    public static CommandResult Success { get; } = new();

    /// <summary>
    /// Factory for domain error.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static CommandResult DomainFail(DomainError error) => new(error);

    /// <summary>
    /// Factory for panic exception.
    /// </summary>
    /// <param name="panicException"></param>
    /// <returns></returns>
    public static CommandResult PanicFail(Exception panicException) => new(panicException);

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
                ? onDomainError(DomainError ?? throw new InvalidOperationException($"{nameof(DomainError)} is null"))
                : IsPanicError
                    ? onPanicError(PanicException ?? throw new InvalidOperationException($"{nameof(PanicException)} is null"))
                    : throw new InvalidOperationException("Invalid state");
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
        if (onPanicError == null) throw new ArgumentNullException(nameof(onPanicError));

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
    /// Evaluates a specified action on success.
    /// </summary>
    /// <param name="success">The action to evaluate if the value is present.</param>
    public void MatchSuccess(Action success)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));

        if (IsSuccess) success();
    }

    /// <summary>
    /// True if success, else false.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator bool(CommandResult result) => result.IsSuccess;

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