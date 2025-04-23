using HerrGeneral.Core;
using HerrGeneral.Test.Extension.Log;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HerrGeneral.Test.Extension;

/// <summary>
/// Extension methods for testing code using HerrGeneral.Core
/// </summary>
public static class Extension
{
    /// <summary>
    /// Send the command with assertion on the success result by default
    /// </summary>
    /// <param name="request"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="withResultAssertion"></param>
    /// <returns></returns>
    public static async Task<Result> Send(this object request, IServiceProvider serviceProvider, bool withResultAssertion = true)
    {
        var res = await serviceProvider.GetRequiredService<Mediator>().Send(request);

        if (withResultAssertion)
            res.IsSuccess.ShouldBeTrue($"{request.GetType().FullName}: {res}");

        return res;
    }

    /// <summary>
    /// Send the creation command and return the Id of the created aggregate
    /// </summary>
    /// <param name="request"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static async Task<TResult> Send<TResult>(this object request, IServiceProvider serviceProvider) =>
        (await serviceProvider
            .GetRequiredService<Mediator>()
            .Send<TResult>(request))
        .Match(id => id,
            domainError => throw new XunitException($"Command have a domain error of type<{domainError.GetType()}>. {domainError.Message}"),
            exception => throw new XunitException($"Command have a panic exception of type<{exception.GetType()}>. {exception.Message}", exception));

    /// <summary>
    /// Send the creation command with assertion on the success result by default
    /// </summary>
    /// <param name="request"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="withResultAssertion"></param>
    /// <returns></returns>
    public static async Task<Result<TResult>> Send<TResult>(this object request, IServiceProvider serviceProvider, bool withResultAssertion)
    {
        var res = await serviceProvider.GetRequiredService<Mediator>().Send<TResult>(request);

        if (withResultAssertion)
            res.IsSuccess.ShouldBeTrue($"{request.GetType().FullName}: {res}");

        return res;
    }

    /// <summary>
    /// Assert for domain exception
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="TDomainError"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task ShouldHaveDomainErrorOfType<TDomainError>(this Task<Result> task) where TDomainError : DomainError
    {
        ArgumentNullException.ThrowIfNull(task);
        (await task)
            .Match(
                () => Assert.Fail($"Command is successful but should have a domain error of type<{typeof(TDomainError)}>."),
                error => error.ShouldBeOfType<TDomainError>($"DomainError<{error.GetType()} is not of type<{typeof(TDomainError)}>."),
                exception => Assert.Fail($"Command should have a domain error of type<{typeof(TDomainError)}> but has panic exception. {exception}"));
    }

    /// <summary>
    /// Assert for domain exception
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="TDomainError"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task ShouldHaveDomainErrorOfType<TResult,TDomainError>(this Task<Result<TResult>> task) where TDomainError : DomainError
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
    /// Assert for panic exception
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="TError"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task ShouldHavePanicExceptionOfType<TError>(this Task<Result> task) where TError : Exception
    {
        ArgumentNullException.ThrowIfNull(task);
        (await task)
            .Match(
                () => Assert.Fail($"Command is successful but should have a panic exception of type<{typeof(TError)}>."),
                error => Assert.Fail($"Command should have a panic exception of type<{typeof(TError)}> but has panic error. {error}"),
                exception => exception.ShouldBeOfType<TError>($"{exception.GetType()} is not of type<{typeof(TError)}>."));
    }

    /// <summary>
    /// Assert for panic exception
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="TError"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task ShouldHavePanicExceptionOfType<TError, TResult>(this Task<Result<TResult>> task) where TError : Exception
    {
        ArgumentNullException.ThrowIfNull(task);
        (await task)
            .Match(
                _ => Assert.Fail($"Command is successful but should have a panic exception of type<{typeof(TError)}>."),
                error => Assert.Fail($"Command should have a panic exception of type<{typeof(TError)}> but has panic error. {error}"),
                exception => exception.ShouldBeOfType<TError>($"{exception.GetType()} is not of type<{typeof(TError)}>."));
    }

    /// <summary>
    /// Register a logger to writes to the xUnit test output.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="testOutputHelper"></param>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public static IServiceCollection AddHerrGeneralTestLogger(this IServiceCollection serviceCollection, ITestOutputHelper testOutputHelper, LogLevel logLevel = LogLevel.Debug) =>
        serviceCollection.AddLogging(builder =>
        {
            builder
                .SetMinimumLevel(logLevel)
                .AddXunit(testOutputHelper, LogConfig.Current);
        });
}