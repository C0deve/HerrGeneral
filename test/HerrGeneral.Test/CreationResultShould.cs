using HerrGeneral.Test.Data.WriteSide;
using HerrGeneral.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Result.Test;

public class CreationResultShould
{
    [Fact]
    public void MatchActionOnSuccess()
    {
        var message = "";
        var expected = Guid.NewGuid();
        
        CreateResult
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

        CreateResult
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

        CreateResult
            .PanicFail(new Exception())
            .Match(
                _ => message = "success",
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe("panicException");
    }

    [Fact]
    public void MatchFunctionOnSuccess() =>
        CreateResult
            .Success(Guid.NewGuid())
            .Match(
                _ => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("success");

    [Fact]
    public void MatchFunctionOnDomainError() =>
        CreateResult
            .DomainFail(new PingError())
            .Match(
                _ => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("domainError");

    [Fact]
    public void MatchFunctionOnPanicException() =>
        CreateResult
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

        CreateResult
            .Success(expected)
            .MatchSuccess(id => message = id.ToString());

        message.ShouldBe(expected.ToString());
    }

    [Fact]
    public void MatchDomainError()
    {
        var message = "";

        CreateResult
            .DomainFail(new PingError())
            .MatchDomainError(_ => message = "domainError");

        message.ShouldBe("domainError");
    }

    [Fact]
    public void MatchPanicException()
    {
        var message = "";

        CreateResult
            .PanicFail(new Exception())
            .MatchPanicException(_ => message = "panicException");

        message.ShouldBe("panicException");
    }
}