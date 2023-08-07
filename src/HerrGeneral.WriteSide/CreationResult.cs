namespace HerrGeneral.WriteSide;

/// <summary>
/// Result of an handled creation command
/// Contain the aggregate id
/// </summary>
public sealed record CreationResult : CommandResultBase
{
    private readonly Guid? _aggregateId;


    private CreationResult(Guid aggregateId) =>
        _aggregateId = aggregateId;

    private CreationResult(DomainError error) : base(error)
    {
    }

    private CreationResult(Exception panicException) : base(panicException)
    {
    }

    /// <summary>
    /// Factory for success
    /// </summary>
    public static CreationResult Success(Guid aggregateId) => new(aggregateId);

    /// <summary>
    /// Factory for domain error
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static CreationResult DomainFail(DomainError error) => new(error);

    /// <summary>
    /// Factory for panic exception
    /// </summary>
    /// <param name="panicException"></param>
    /// <returns></returns>
    public static CreationResult PanicFail(Exception panicException) => new(panicException);

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
            ? onSuccess(_aggregateId ?? throw new InvalidOperationException($"{nameof(_aggregateId)} is null"))
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
    public void Match(Action<Guid> onSuccess, Action<DomainError> onDomainError, Action<Exception> onPanicError)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));
        if (onPanicError == null) throw new ArgumentNullException(nameof(onPanicError));

        if (IsSuccess)
            onSuccess(_aggregateId ?? throw new InvalidOperationException($"{nameof(_aggregateId)} is null"));
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
    public void MatchSuccess(Action<Guid> success)
    {
        if (success == null) throw new ArgumentNullException(nameof(success));

        if (IsSuccess) success(_aggregateId ?? throw new InvalidOperationException($"{nameof(_aggregateId)} is null"));
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