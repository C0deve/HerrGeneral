namespace HerrGeneral.WriteSide;

/// <summary>
/// Extension for railway programming
/// </summary>
public static class CommandResultExtensions
{
    /// <summary>
    /// Chaining operations returning a result 
    /// </summary>
    /// <param name="changeResult"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static ChangeResult Then(this ChangeResult changeResult, Func<ChangeResult> func) =>
        changeResult.Match(func, _ => changeResult, _ => changeResult);

    /// <summary>
    /// Chaining async operations returning a result 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<ChangeResult> Then(this Task<ChangeResult> task, Func<Task<ChangeResult>> func)
    {
        var changeResult = await task;
        return await changeResult
            .Match(
                func,
                _ => Task.FromResult(changeResult),
                _ => Task.FromResult(changeResult));
    }

    /// <summary>
    /// Chaining async operations returning a result 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<ChangeResult> Then(this Task<CreateResult> task, Func<Guid, Task<ChangeResult>> func)
    {
        var createResult = await task;
        return await createResult
            .Match(
                func,
                domainError => Task.FromResult(ChangeResult.DomainFail(domainError)),
                exception => Task.FromResult(ChangeResult.PanicFail(exception)));
    }

    /// <summary>
    /// Evaluates a specified action, after a task returning a result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onDomainError"></param>
    /// <param name="onPanicError"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task Match(this Task<ChangeResult> task, Action onSuccess, Action<DomainError> onDomainError, Action<Exception> onPanicError)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));
        if (onPanicError == null) throw new ArgumentNullException(nameof(onPanicError));

        (await task).Match(onSuccess, onDomainError, onPanicError);
    }

    /// <summary>
    /// Evaluates a specified function, after a task returning a result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="onSuccess">The function to evaluate on success.</param>
    /// <param name="onDomainError">The function to evaluate on domain error.</param>
    /// <param name="onPanicError">The function to evaluate on panic error.</param>
    /// <returns>The result of the evaluated function.</returns>
    public static async Task<TResult> Match<TResult>(this Task<ChangeResult> task, Func<TResult> onSuccess, Func<DomainError, TResult> onDomainError, Func<Exception, TResult> onPanicError)
    {
        if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
        if (onDomainError == null) throw new ArgumentNullException(nameof(onDomainError));
        if (onPanicError == null) throw new ArgumentNullException(nameof(onPanicError));

        return (await task).Match(onSuccess, onDomainError, onPanicError);
    }
}