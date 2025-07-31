using HerrGeneral.Core;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.CommandResult.Test;

public class ResultWithReturnValueShould
{
    [Fact]
    public void MatchActionOnSuccess()
    {
        var message = "";
        var expected = Guid.NewGuid();
        
        Result
            .Success(expected)
            .Match(
                id => message = id.ToString(),
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe(expected.ToString());
    }

    [Fact]
    public void MatchActionOnDomainError()
    {
        var message = "";

        Result<Guid>
            .DomainFail(new PingError())
            .Match(
                _ => message = "success",
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe("domainError");
    }

    [Fact]
    public void MatchActionOnPanicException()
    {
        var message = "";

        Result<Guid>
            .PanicFail(new Exception())
            .Match(
                _ => message = "success",
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe("panicException");
    }

    [Fact]
    public void MatchFunctionOnSuccess() =>
        Result
            .Success(Guid.NewGuid())
            .Match(
                _ => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("success");

    [Fact]
    public void MatchFunctionOnDomainError() =>
        Result<Guid>
            .DomainFail(new PingError())
            .Match(
                _ => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("domainError");

    [Fact]
    public void MatchFunctionOnPanicException() =>
        Result<Guid>
            .PanicFail(new Exception())
            .Match(
                _ => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("panicException");

    [Fact]
    public void MatchSuccess()
    {
        var message = "";
        var expected = Guid.NewGuid();

        Result
            .Success(expected)
            .MatchSuccess(id => message = id.ToString());

        message.ShouldBe(expected.ToString());
    }

    [Fact]
    public void MatchDomainError()
    {
        var message = "";

        Result<Guid>
            .DomainFail(new PingError())
            .MatchDomainError(_ => message = "domainError");

        message.ShouldBe("domainError");
    }

    [Fact]
    public void MatchPanicException()
    {
        var message = "";

        Result<Guid>
            .PanicFail(new Exception())
            .MatchPanicException(_ => message = "panicException");

        message.ShouldBe("panicException");
    }
}