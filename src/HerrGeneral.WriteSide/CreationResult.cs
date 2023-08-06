namespace HerrGeneral.WriteSide;

/// <summary>
/// Result of an handled creation command
/// Contain the aggregate id
/// </summary>
public sealed class CreationResult : IWithSuccess
{
    /// <summary>
    /// Id of the created aggregate
    /// </summary>
    public Guid AggregateId { get; }

    private readonly Exception? _panicException;
    private readonly DomainError? _domainError;

    /// <summary>
    /// The operation was successful.
    /// </summary>
    public bool IsSuccess => !IsDomainError && !IsPanicError;
    private bool IsDomainError => _domainError != null;
    private bool IsPanicError => _panicException != null;

    private CreationResult(Guid aggregateId) => 
        AggregateId = aggregateId;

    private CreationResult(DomainError error) => 
        _domainError = error;

    private CreationResult(Exception panicException) => 
        _panicException = panicException;

    /// <summary>
    /// Factory for success
    /// </summary>
    public static CreationResult Success(Guid aggregateId) => new CreationResult(aggregateId);

    /// <summary>
    /// Factory for domain error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static CreationResult DomainFail(DomainError error) => new CreationResult(error);

    /// <summary>
    /// Factory for panic exception
    /// </summary>
    /// <param name="panicException"></param>
    /// <returns></returns>
    public static CreationResult PanicFail(Exception panicException) => new CreationResult(panicException);

    /// <summary>
    /// Evaluates a specified function, based on the .current state
    /// </summary>
    /// <param name="onSuccess">The function to evaluate on success.</param>
    /// <param name="onDomainError">The function to evaluate on domain error.</param>
    /// <param name="onPanicError">The function to evaluate on panic error.</param>
    /// <returns>The result of the evaluated function.</returns>
    public TResult Match<TResult>(Func<Guid, TResult> onSuccess, Func<DomainError, TResult> onDomainError, Func<Exception, TResult> onPanicError)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));
        if (onPanicError == null) throw new ArgumentNullException(nameof(onPanicError));
         
        return IsSuccess
            ? onSuccess(AggregateId)
            : IsDomainError
                ? onDomainError(_domainError ?? throw new InvalidOperationException())
                : onPanicError(_panicException ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Evaluates a specified action based on the .current state
    /// </summary>
    /// <param name="onSuccess">The action to evaluate if the value is present.</param>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    /// <param name="onPanicError"></param>
    public void Match(Action<Guid> onSuccess, Action<DomainError> onDomainError, Action<Exception> onPanicError)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));

        if (IsSuccess)
            onSuccess(AggregateId);
        else if (IsDomainError)
            onDomainError(_domainError ?? throw new InvalidOperationException());
        else
            onPanicError(_panicException ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Evaluates a specified action based on the .current state
    /// </summary>
    /// <param name="success">The action to evaluate if the value is present.</param>
    public void MatchSuccess(Action<Guid> success)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));

        if (IsSuccess) success(AggregateId);
    }

    /// <summary>
    /// Evaluates a specified action based on the .current state.
    /// </summary>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    public void MatchDomainError(Action<DomainError> onDomainError)
    {
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));

        if (IsDomainError) onDomainError(_domainError ?? throw new InvalidOperationException());
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

        if (IsPanicError) onPanicException(_panicException ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// True if success, else false.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static implicit operator bool(CreationResult result) => result.IsSuccess;

    /// <summary>
    /// Display the result
    /// </summary>
    /// <returns></returns>
    public override string ToString() =>
        Match(
            id => $"Id<{id}>",
            error => error.Message,
            exception => exception.Message
        );
}