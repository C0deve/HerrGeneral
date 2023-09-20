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

    [Fact]
    public void ChangeResultRailwaySuccess() =>
        ChangeResult.Success
            .Then(() => ChangeResult.Success)
            .ShouldBe(ChangeResult.Success);

    [Fact]
    public void ChangeResultRailwayDomainError() =>
        ChangeResult.DomainFail(new PingError())
            .Then(() => ChangeResult.Success)
            .IsSuccess.ShouldBe(false);

    [Fact]
    public void ChangeResultRailwayDomainError2() =>
        ChangeResult.Success
            .Then(() => ChangeResult.DomainFail(new PingError()))
            .IsSuccess.ShouldBe(false);

    [Fact]
    public async Task ChangeResultRailwayDomainError3() =>
        (await Task.FromResult(ChangeResult.Success)
            .Then(() => Task.FromResult(ChangeResult.DomainFail(new PingError()))))
        .IsSuccess.ShouldBe(false);

    [Fact]
    public async Task ChangeResultRailwayDomainError4() =>
        (await Task.FromResult(ChangeResult.DomainFail(new PingError()))
            .Then(() => Task.FromResult(ChangeResult.Success)))
        .IsSuccess.ShouldBe(false);

    [Fact]
    public async Task CreateResultRailwaySuccess() =>
        (await Task.FromResult(CreateResult.Success(Guid.NewGuid()))
            .Then(_ => Task.FromResult(ChangeResult.Success)))
        .ShouldBe(ChangeResult.Success);
    
    [Fact]
    public async Task CreateResultRailwayDomainError() =>
        (await Task.FromResult(CreateResult.DomainFail(new PingError()))
            .Then(_ => Task.FromResult(ChangeResult.Success)))
        .IsSuccess.ShouldBe(false);
    
    [Fact]
    public async Task CreateResultRailwayDomainError2() =>
        (await Task.FromResult(CreateResult.Success(Guid.NewGuid()))
            .Then(_ => Task.FromResult(ChangeResult.DomainFail(new PingError()))))
        .IsSuccess.ShouldBe(false);
}