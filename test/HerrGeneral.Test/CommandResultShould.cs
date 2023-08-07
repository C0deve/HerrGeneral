using HerrGeneral.Test.Data.WriteSide;
using HerrGeneral.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Result.Test;

public class CommandResultShould
{
    [Fact]
    public void MatchActionOnSuccess()
    {
        var message = "";

        CommandResult
            .Success
            .Match(
                () => message = "success",
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe("success");
    }

    [Fact]
    public void MatchActionOnDomainError()
    {
        var message = "";

        CommandResult
            .DomainFail(new PingError())
            .Match(
                () => message = "success",
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe("domainError");
    }

    [Fact]
    public void MatchActionOnPanicException()
    {
        var message = "";

        CommandResult
            .PanicFail(new Exception())
            .Match(
                () => message = "success",
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe("panicException");
    }

    [Fact]
    public void MatchFunctionOnSuccess() =>
        CommandResult
            .Success
            .Match(
                () => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("success");

    [Fact]
    public void MatchFunctionOnDomainError() =>
        CommandResult
            .DomainFail(new PingError())
            .Match(
                () => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("domainError");

    [Fact]
    public void MatchFunctionOnPanicException() =>
        CommandResult
            .PanicFail(new Exception())
            .Match(
                () => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("panicException");

    [Fact]
    public void MatchSuccess()
    {
        var message = "";

        CommandResult.Success.MatchSuccess(() => message = "success");

        message.ShouldBe("success");
    }

    [Fact]
    public void MatchDomainError()
    {
        var message = "";

        CommandResult
            .DomainFail(new PingError())
            .MatchDomainError(_ => message = "domainError");

        message.ShouldBe("domainError");
    }

    [Fact]
    public void MatchPanicException()
    {
        var message = "";

        CommandResult
            .PanicFail(new Exception())
            .MatchPanicException(_ => message = "panicException");

        message.ShouldBe("panicException");
    }
}