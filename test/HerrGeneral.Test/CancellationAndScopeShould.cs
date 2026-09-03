using HerrGeneral.Registration;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.CancellationAndScope.Test;

public class CancellationAndScopeShould(ITestOutputHelper output)
{
    private record CancelCmd;
    private class CancelHandler : ICommandHandler<CancelCmd, Unit>
    {
        public (IEnumerable<object> Events, Unit Result) Handle(CancelCmd command) => ([], Unit.Default);
    }

    private record ScopedCmd;
    private class ScopedDependency
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    private class ScopedHandler(ScopedDependency dependency) : ICommandHandler<ScopedCmd, Guid>
    {
        public (IEnumerable<object> Events, Guid Result) Handle(ScopedCmd command) => ([], dependency.Id);
    }

    [Fact]
    public async Task Propagate_cancellation_token()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(CancelHandler).Assembly, typeof(CancelHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await mediator.Send(new CancelCmd(), cts.Token);
        });
    }

    [Fact]
    public async Task Reuse_ambient_scope_for_scoped_dependencies()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddScoped<ScopedDependency>()
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(ScopedHandler).Assembly, typeof(ScopedHandler).Namespace!));

        var rootProvider = services.BuildServiceProvider();
        using var scope = rootProvider.CreateScope();

        var scopedDependency = scope.ServiceProvider.GetRequiredService<ScopedDependency>();
        var mediator = scope.ServiceProvider.GetRequiredService<Mediator>();

        var result = await mediator.Send<Guid>(new ScopedCmd());

        result.Match(
            onSuccess: id => id.ShouldBe(scopedDependency.Id),
            onDomainError: _ => Assert.Fail("Should succeed"),
            onPanicError: ex => throw ex);
    }
}
