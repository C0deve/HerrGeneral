using HerrGeneral.Core;
using HerrGeneral.Test.Extension.Log;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

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
    public static async Task<CommandResultV2> Send(this ICommand<CommandResultV2> request, IServiceProvider serviceProvider, bool withResultAssertion = true)
    {
        var res = await serviceProvider.GetRequiredService<Mediator>().Send(request);

        if (withResultAssertion)
            res.IsSuccess.ShouldBe(true, $"{request.GetType().FullName}: {res}");

        return res;
    }

    /// <summary>
    /// Send the creation command with assertion on the success result by default
    /// </summary>
    /// <param name="request"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="withResultAssertion"></param>
    /// <returns></returns>
    public static async Task<Guid> Send(this ICommand<CreationResult> request, IServiceProvider serviceProvider, bool withResultAssertion = true)
    {
        var res = await serviceProvider.GetRequiredService<Mediator>().Send(request);

        if (withResultAssertion)
            res.IsSuccess.ShouldBe(true, $"{request.GetType().FullName}: {res}");

        return res.AggregateId;
    }

    /// <summary>
    /// Assert for domain exception
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="TDomainError"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task ShouldHaveDomainErrorOfType<TDomainError>(this Task<CommandResultV2> task) where TDomainError : DomainError
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        (await task)
            .Match(
                () => Assert.Fail($"Command is successful but should have domain error of type<{typeof(TDomainError)}>"),
                error => error.ShouldBeOfType<TDomainError>($"DomainError<{error.GetType()} is not of type<{typeof(TDomainError)}>"),
                exception => Assert.Fail($"Command should have domain error of type<{typeof(TDomainError)}> but has panic error. {exception}")
            );
    }

    /// <summary>
    /// Assert for panic exception
    /// </summary>
    /// <param name="task"></param>
    /// <typeparam name="TError"></typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task ShouldHavePanicExceptionOfType<TError>(this Task<CommandResultV2> task) where TError : Exception
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        (await task)
            .Match(
                () => Assert.Fail($"Command is successful but should have domain error of type<{typeof(TError)}>"),
                error => Assert.Fail($"Command should have domain error of type<{typeof(TError)}> but has panic error. {error}"),
                exception => exception.ShouldBeOfType<TError>($"{exception.GetType()} is not of type<{typeof(TError)}>"));
    }

    /// <summary>
    /// Register a logger to writes to the xUnit test output.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="testOutputHelper"></param>
    /// <returns></returns>
    public static IServiceCollection AddHerrGeneralTestLogger(this IServiceCollection serviceCollection, ITestOutputHelper testOutputHelper) =>
        serviceCollection.AddLogging(builder => { builder.AddXunit(testOutputHelper, LogConfig.Current); });
}