using HerrGeneral.Error;

namespace HerrGeneral.WriteSide;

public class CreationResult : IWithSuccess
{
    public Guid AggregateId { get; }
    public Exception? PanicException { get; }
    public DomainError? DomainError { get; }

    public bool IsSuccess => !IsDomainError && !IsPanicError;
    private bool IsDomainError => DomainError != null;
    private bool IsPanicError => PanicException != null;

    private CreationResult(Guid aggregateId)
    {
        AggregateId = aggregateId;
    }

    private CreationResult(DomainError error)
    {
        DomainError = error;
    }

    private CreationResult(Exception panicException)
    {
        PanicException = panicException;
    }

    public static CreationResult Success(Guid aggregateId) => new CreationResult(aggregateId);

    public static CreationResult DomainFail(DomainError error) => new CreationResult(error);

    public static CreationResult PanicFail(Exception panicException) => new CreationResult(panicException);

    /// <summary>
    /// Evaluates a specified function, based on whether a value is present or not.
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
                ? onDomainError(DomainError ?? throw new InvalidOperationException())
                : onPanicError(PanicException ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Evaluates a specified action, based on whether a value is present or not.
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
            onDomainError(DomainError ?? throw new InvalidOperationException());
        else
            onPanicError(PanicException ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Evaluates a specified action if a value is present.
    /// </summary>
    /// <param name="success">The action to evaluate if the value is present.</param>
    public void MatchSuccess(Action<Guid> success)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));

        if (IsSuccess) success(AggregateId);
    }

    /// <summary>
    /// Evaluates a specified action if no value is present.
    /// </summary>
    /// <param name="onDomainError">The action to evaluate if the value is missing.</param>
    public void MatchDomainError(Action<DomainError> onDomainError)
    {
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));

        if (IsDomainError) onDomainError(DomainError ?? throw new InvalidOperationException());
    }
        
    public void MatchPanicException(Action<Exception> onPanicException)
    {
        if (onPanicException == null) throw new ArgumentNullException(nameof(onPanicException));

        if (IsPanicError) onPanicException(PanicException ?? throw new InvalidOperationException());
    }

    public static implicit operator bool(CreationResult result) => result.IsSuccess;

    public override string ToString() =>
        Match(
            id => $"Id<{id}>",
            error => error.Message,
            exception => exception.Message
        );
}