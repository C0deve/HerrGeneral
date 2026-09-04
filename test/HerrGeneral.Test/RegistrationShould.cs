using HerrGeneral.Core.ReadSide;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Registration.Test;

public class RegistrationShould(ITestOutputHelper output)
{
    private record Ping;

    private class PingHandler(Dependency dependency) : ICommandHandler<Ping, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(Ping command)
        {
            dependency.Called = true;
            return ([], Unit.Default);
        }
    }

    private class Dependency
    {
        public bool Called { get; set; }
    }

    [Fact]
    public async Task Resolve_main_handler()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(PingHandler).Assembly, typeof(PingHandler).Namespace!));

        services.AddSingleton<Dependency>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        var response = await mediator.Send(new Ping());

        response.ShouldBe(Result.Success());
        serviceProvider.GetRequiredService<Dependency>().Called.ShouldBe(true);
    }

    [Fact]
    public async Task Raise_exception_if_no_command_handler_registered()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(PingHandler).Assembly, "empty.namespace"));

        services.AddSingleton<Dependency>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        await Should.ThrowAsync<MissingCommandHandlerRegistrationException>(async () => await mediator.Send(new Ping()));
    }

    [Fact]
    public void Resolve_handlers_when_a_class_implements_multiple_handlers()
    {
        var services = new ServiceCollection()
            .AddSingleton<AReadModelWithMultipleHandlers>()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(scanner =>
                scanner
                    .ScanReadSideOn(typeof(AReadModelWithMultipleHandlers).Assembly, typeof(AReadModelWithMultipleHandlers).Namespace!)
                    .RegisterReadSideEventHandler<EventBase, HerrGeneral.Test.Data.WithMapping.ReadSide.ILocalEventHandler<EventBase>>());

        var container = services.BuildServiceProvider();

        container
            .GetRequiredService<ReadSide.IProjectionEventHandler<AnotherPong>>()
            .ShouldBeOfType<ProjectionEventHandlerWithMapping<AnotherPong, AReadModelWithMultipleHandlers>>();

        container
            .GetRequiredService<ReadSide.IProjectionEventHandler<Pong>>()
            .ShouldBeOfType<ProjectionEventHandlerWithMapping<Pong, AReadModelWithMultipleHandlers>>();
    }

    [Fact]
    public void Register_read_side_repositories_as_singleton()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(scanner =>
                scanner
                    .ScanReadSideOn(typeof(AReadModelWithMultipleHandlers).Assembly, typeof(AReadModelWithMultipleHandlers).Namespace!)
                    .RegisterReadSideEventHandler<EventBase, HerrGeneral.Test.Data.WithMapping.ReadSide.ILocalEventHandler<EventBase>>());


        var container = services.BuildServiceProvider();

        container.GetRequiredService<AReadModel>().Id.ShouldBe(container.GetRequiredService<AReadModel>().Id);
    }

    private record MultiCmd1;
    private record MultiCmd2;

    private class MultiCommandHandler : ICommandHandler<MultiCmd1, Unit>, ICommandHandler<MultiCmd2, string>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(MultiCmd1 command) => ([], Unit.Default);
        public (IReadOnlyList<object> Events, string Result) Handle(MultiCmd2 command) => ([], "Success");
    }

    [Fact]
    public async Task Resolve_multiple_command_handlers_in_a_single_class()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(MultiCommandHandler).Assembly, typeof(MultiCommandHandler).Namespace!));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        var res1 = await mediator.Send(new MultiCmd1());
        var res2 = await mediator.Send<string>(new MultiCmd2());

        res1.ShouldBe(Result.Success());
        res2.Match(
            onSuccess: val => val.ShouldBe("Success"),
            onDomainError: _ => Assert.Fail("Expected success"),
            onPanicError: ex => throw ex);
    }

    [Fact]
    public void Throw_when_duplicate_command_handler_registered_for_same_command()
    {
        var services = new ServiceCollection();
        var policy = new HerrGeneral.Core.Registration.Policy.RegisterICommandHandler();
        var externalHandlers = new Dictionary<Type, HashSet<Type>>
        {
            [typeof(ICommandHandler<,>)] = [typeof(HerrGeneral.Test.Data.Duplicates.DuplicateHandlerA), typeof(HerrGeneral.Test.Data.Duplicates.DuplicateHandlerB)]
        };

        var ex = Should.Throw<InvalidOperationException>(() => policy.Register(services, externalHandlers));
        ex.Message.ShouldContain("already has a registered handler");
    }

    [Fact]
    public async Task LimitConcurrentCommands_allows_multiple_parallel_executions()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(scanner =>
                scanner
                    .ScanWriteSideOn(typeof(PingHandler).Assembly, typeof(PingHandler).Namespace!)
                    .LimitConcurrentCommandsTo(3));

        services.AddSingleton<Dependency>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        var tasks = Enumerable.Range(0, 3).Select(_ => mediator.Send(new Ping())).ToArray();
        var results = await Task.WhenAll(tasks);

        results.ShouldAllBe(r => r == Result.Success());
    }
}