using Shouldly;
using Xunit;
using Xunit.Sdk;

namespace HerrGeneral.Test;

/// <summary>
/// Extensions methods for <see cref="Result"/>
/// </summary>
public static class ResultExtensions
{
    /// <param name="command">The command to send</param>
    extension(object command)
    {
        /// <summary>
        /// Sends a command through the mediator and asserts that the execution is successful.
        /// This method is useful for testing command handlers when you don't need to check the return value.
        /// </summary>
        /// <param name="mediator">The mediator instance used to process the command</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public Task AssertSendFrom(Mediator mediator) => 
            mediator
                .Send(command)
                .ShouldSuccess();

        /// <summary>
        /// Sends a command through the mediator, asserts that the execution is successful, and returns the result value.
        /// This method is useful for testing command handlers that return a value (e.g., an entity ID).
        /// </summary>
        /// <param name="mediator">The mediator instance used to process the command</param>
        /// <typeparam name="TValue">The type of value expected in the successful result</typeparam>
        /// <returns>The value from the successful result</returns>
        public Task<TValue> AssertSendFrom<TValue>(Mediator mediator) => 
            mediator
                .Send<TValue>(command)
                .ShouldSuccess();
    }

    /// <summary>
    /// Asserts that the result is successful
    /// </summary>
    /// <param name="result"></param>
    public static async Task ShouldSuccess(this Task<Result> result) =>
        (await result).Match(
            _ => {},
            domainError => throw new XunitException($"Command have a domain error of type<{domainError.GetType()}>. {domainError}"),
            exception => throw new XunitException($"Command have a panic exception of type<{exception.GetType()}>. {exception.Message}", exception));

    
    /// <param name="task"></param>
    extension(Task<Result> task)
    {
        /// <summary>
        /// Assert for panic exception
        /// </summary>
        /// <typeparam name="TError"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task ShouldFailWithPanicExceptionOfType<TError>() where TError : System.Exception
        {
            ArgumentNullException.ThrowIfNull(task);
            (await task)
                .Match(
                    () => Assert.Fail($"Command is successful but should have a panic exception of type<{typeof(TError)}>."),
                    error => Assert.Fail($"Command should have a panic exception of type<{typeof(TError)}> but has panic error. {error}"),
                    exception => exception.ShouldBeOfType<TError>($"{exception.GetType()} is not of type<{typeof(TError)}>."));
        }

        /// <summary>
        /// Assert for domain exception
        /// </summary>
        /// <typeparam name="TDomainError"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task ShouldFailWithDomainErrorOfType<TDomainError>()
        {
            ArgumentNullException.ThrowIfNull(task);
            (await task)
                .Match(
                    () => Assert.Fail($"Command is successful but should have a domain error of type<{typeof(TDomainError)}>."),
                    error => error.ShouldBeOfType<TDomainError>($"DomainError<{error.GetType()} is not of type<{typeof(TDomainError)}>."),
                    exception => Assert.Fail($"Command should have a domain error of type<{typeof(TDomainError)}> but has panic exception. {exception}"));
        }
    }
    
    /// <param name="task"></param>
    /// <typeparam name="TValue"></typeparam>
    extension<TValue>(Task<Result<TValue>> task)
    {
        /// <summary>
        /// Asserts that the task is successful
        /// </summary>
        /// <returns></returns>
        /// <exception cref="XunitException"></exception>
        public async Task<TValue> ShouldSuccess() =>
            (await task)
            .Match(id => id,
                domainError => throw new XunitException($"Command have a domain error of type<{domainError.GetType()}>. {domainError}"),
                exception => throw new XunitException($"Command have a panic exception of type<{exception.GetType()}>. {exception.Message}", exception));
    
        /// <summary>
        /// Asserts that the task is successful and contains the expected value.
        /// </summary>
        /// <param name="expected">The expected value to be contained in the result</param>
        /// <returns>The value from the result if successful</returns>
        /// <exception cref="XunitException">Thrown when the result is not successful or doesn't contain the expected value</exception>
        public async Task ShouldSuccessWithValue(TValue expected)
        {
            var res = await task.ShouldSuccess();
            res.ShouldBe(expected);
        }

        /// <summary>
        /// Asserts that a result contains a domain error of the specified type.
        /// </summary>
        /// <typeparam name="TDomainError">The expected type of domain error</typeparam>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when the task is null</exception>
        /// <exception cref="XunitException">Thrown when the result doesn't contain a domain error of the expected type</exception>
        public async Task ShouldFailWithDomainErrorOfType<TDomainError>()
        {
            ArgumentNullException.ThrowIfNull(task);
            (await task)
                .Match(
                    _ => Assert.Fail($"Command is successful but should have a domain error of type<{typeof(TDomainError)}>."),
                    error => error.ShouldBeOfType<TDomainError>($"DomainError<{error.GetType()} is not of type<{typeof(TDomainError)}>."),
                    exception => Assert.Fail($"Command should have a domain error of type<{typeof(TDomainError)}> but has panic exception. {exception}")
                );
        }

        /// <summary>
        /// Asserts that a result with a value contains a panic exception of the specified type.
        /// </summary>
        /// <typeparam name="TError">The expected type of the exception</typeparam>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">Thrown when the task is null</exception>
        /// <exception cref="XunitException">Thrown when the result doesn't contain a panic exception of the expected type</exception>
        public async Task ShouldFailWithPanicExceptionOfType<TError>() where TError : System.Exception
        {
            ArgumentNullException.ThrowIfNull(task);
            (await task)
                .Match(
                    _ => Assert.Fail($"Command is successful but should have a panic exception of type<{typeof(TError)}>."),
                    error => Assert.Fail($"Command should have a panic exception of type<{typeof(TError)}> but has panic error. {error}"),
                    exception => exception.ShouldBeOfType<TError>($"{exception.GetType()} is not of type<{typeof(TError)}>."));
        }

    }
}