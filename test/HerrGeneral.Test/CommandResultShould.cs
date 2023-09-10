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

        ChangeResult
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

        ChangeResult
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

        ChangeResult
            .PanicFail(new Exception())
            .Match(
                () => message = "success",
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe("panicException");
    }

    [Fact]
    public void MatchFunctionOnSuccess() =>
        ChangeResult
            .Success
            .Match(
                () => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("success");

    [Fact]
    public void MatchFunctionOnDomainError() =>
        ChangeResult
            .DomainFail(new PingError())
            .Match(
                () => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("domainError");

    [Fact]
    public void MatchFunctionOnPanicException() =>
        ChangeResult
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

        ChangeResult.Success.MatchSuccess(() => message = "success");

        message.ShouldBe("success");
    }

    [Fact]
    public void MatchDomainError()
    {
        var message = "";

        ChangeResult
            .DomainFail(new PingError())
            .MatchDomainError(_ => message = "domainError");

        message.ShouldBe("domainError");
    }

    [Fact]
    public void MatchPanicException()
    {
        var message = "";

        ChangeResult
            .PanicFail(new Exception())
            .MatchPanicException(_ => message = "panicException");

        message.ShouldBe("panicException");
    }
}