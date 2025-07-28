namespace HerrGeneral.Core;

/// <summary>
/// Extension for railway programming
/// </summary>
public static class CommandResultExtensions
{
    /// <summary>
    /// Chaining operations returning a result 
    /// </summary>
    /// <param name="result"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static Result Then(this Result result, Func<Result> func) =>
        result.Match(func, _ => result, _ => result);

    /// <summary>
    /// Chaining async operations returning a result 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static async Task<Result> Then(this Task<Result> task, Func<Task<Result>> func)
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
    public static async Task<Result> Then<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<Result>> func)
    {
        var result = await task;
        return await result
            .Match(
                func,
                domainError => Task.FromResult(Result.DomainFail(domainError)),
                exception => Task.FromResult(Result.PanicFail(exception)));
    }

    /// <summary>
    /// Evaluates a specified action, after a task returning a result.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onDomainError"></param>
    /// <param name="onPanicError"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task Match(this Task<Result> task, Action onSuccess, Action<object> onDomainError, Action<Exception> onPanicError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onDomainError);
        ArgumentNullException.ThrowIfNull(onPanicError);

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
    public static async Task<TResult> Match<TResult>(this Task<Result> task, Func<TResult> onSuccess, Func<object, TResult> onDomainError, Func<Exception, TResult> onPanicError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onDomainError);
        ArgumentNullException.ThrowIfNull(onPanicError);

        return (await task).Match(onSuccess, onDomainError, onPanicError);
    }
}