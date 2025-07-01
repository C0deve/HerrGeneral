using HerrGeneral.Core;
using HerrGeneral.Test.Data.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.CommandResult.Test;

public class ResultShould
{
    [Fact]
    public void MatchActionOnSuccess()
    {
        var message = "";

        Result
            .Success()
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

        Result
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

        Result
            .PanicFail(new Exception())
            .Match(
                () => message = "success",
                _ => message = "domainError",
                _ => message = "panicException");

        message.ShouldBe("panicException");
    }

    [Fact]
    public void MatchFunctionOnSuccess() =>
        Result
            .Success()
            .Match(
                () => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("success");

    [Fact]
    public void MatchFunctionOnDomainError() =>
        Result
            .DomainFail(new PingError())
            .Match(
                () => "success",
                _ => "domainError",
                _ => "panicException")
            .ShouldBe("domainError");

    [Fact]
    public void MatchFunctionOnPanicException() =>
        Result
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

        Result.Success().MatchSuccess(_ => message = "success");

        message.ShouldBe("success");
    }

    [Fact]
    public void MatchDomainError()
    {
        var message = "";

        Result
            .DomainFail(new PingError())
            .MatchDomainError(_ => message = "domainError");

        message.ShouldBe("domainError");
    }

    [Fact]
    public void MatchPanicException()
    {
        var message = "";

        Result
            .PanicFail(new Exception())
            .MatchPanicException(_ => message = "panicException");

        message.ShouldBe("panicException");
    }

    [Fact]
    public void ChangeResultRailwaySuccess() =>
        Result.Success()
            .Then(Result.Success)
            .ShouldBe(Result.Success());

    [Fact]
    public void ChangeResultRailwayDomainError() =>
        Result.DomainFail(new PingError())
            .Then(Result.Success)
            .IsSuccess.ShouldBe(false);

    [Fact]
    public void ChangeResultRailwayDomainError2() =>
        Result.Success()
            .Then(() => Result.DomainFail(new PingError()))
            .IsSuccess.ShouldBe(false);

    [Fact]
    public async Task ChangeResultRailwayDomainError3() =>
        (await Task.FromResult(Result.Success())
            .Then(() => Task.FromResult(Result.DomainFail(new PingError()))))
        .IsSuccess.ShouldBe(false);

    [Fact]
    public async Task ChangeResultRailwayDomainError4() =>
        (await Task.FromResult(Result.DomainFail(new PingError()))
            .Then(() => Task.FromResult(Result.Success())))
        .IsSuccess.ShouldBe(false);

    [Fact]
    public async Task CreateResultRailwaySuccess() =>
        (await Task.FromResult(Result.Success(Guid.NewGuid()))
            .Then(_ => Task.FromResult(Result.Success())))
        .ShouldBe(Result.Success());
    
    [Fact]
    public async Task CreateResultRailwayDomainError() =>
        (await Task.FromResult(Result<Guid>.DomainFail(new PingError()))
            .Then(_ => Task.FromResult(Result.Success())))
        .IsSuccess.ShouldBe(false);
    
    [Fact]
    public async Task CreateResultRailwayDomainError2() =>
        (await Task.FromResult(Result<Guid>.Success(Guid.NewGuid()))
            .Then(_ => Task.FromResult(Result.DomainFail(new PingError()))))
        .IsSuccess.ShouldBe(false);
}