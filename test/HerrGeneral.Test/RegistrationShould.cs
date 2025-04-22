using HerrGeneral.Core;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Data.ReadSide;
using HerrGeneral.Test.Data.WriteSide;
using HerrGeneral.WriteSide;
using Lamar;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Registration.Test;

public class RegistrationShould
{
    private readonly ITestOutputHelper _output;

    public RegistrationShould(ITestOutputHelper output) => _output = output;

    private record Ping : Change;


    private class PingHandler(Dependency dependency, IEventDispatcher eventDispatcher) : ChangeHandler<Ping>()
    {
        private readonly Dependency _dependency;

        public PingHandler(Dependency dependency, IEventDispatcher eventDispatcher) : base(eventDispatcher) =>
            _dependency = dependency;

        public override Task<ChangeResult> Handle(Ping command, CancellationToken cancellationToken)
        {
            _dependency.Called = true;
            return Task.FromResult(ChangeResult.Success);
        }
    }
    
    private class Dependency
    {
        public bool Called { get; set; }
    }
    
    [Fact]
    public async Task Resolve_main_handler()
    {
        var container = new Container(cfg =>
        {
            cfg.ForSingletonOf<Dependency>().Use(new Dependency());

            cfg.AddHerrGeneralTestLogger(_output);

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .AddWriteSideAssembly(typeof(PingHandler).Assembly, typeof(PingHandler).Namespace!));
        });

        var mediator = container.GetInstance<Mediator>();

        var response = await mediator.Send(new Ping());

        response.ShouldBe(ChangeResult.Success);
        container.GetInstance<Dependency>().Called.ShouldBe(true);
    }
    
    [Fact]
    public async Task Raise_exception_if_no_command_handler_registered()
    {
        var container = new Container(cfg =>
        {
            cfg.ForSingletonOf<Dependency>().Use(new Dependency());

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .AddWriteSideAssembly(typeof(PingHandler).Assembly, "empty.namespace"));
        });

        var mediator = container.GetInstance<Mediator>();

        await Should.ThrowAsync<MissingCommandHandlerRegistrationException>(async () => await mediator.Send(new Ping ()));
    }
    
    [Fact]
    public void Resolve_handlers_when_a_class_implements_multiple_handlers() {
        var container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(_output);

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .AddReadSideAssembly(typeof(ReadModelWithMultipleHandlers).Assembly, typeof(ReadModelWithMultipleHandlers).Namespace!));
        });

        container
            .GetInstance<ReadSide.IEventHandler<AnotherPong>>()
            .ShouldBeOfType<ReadModelWithMultipleHandlers.Repository>();
        
        container
            .GetInstance<ReadSide.IEventHandler<Pong>>()
            .ShouldBeOfType<ReadModelWithMultipleHandlers.Repository>();
    }
    
    [Fact]
    public void Register_read_side_repositories_as_singleton()
    {
        var container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(_output);

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .AddReadSideAssembly(typeof(ReadModelWithMultipleHandlers).Assembly, typeof(ReadModelWithMultipleHandlers).Namespace!));
        });
        
        container.GetInstance<ReadModel.Repository>().Id.ShouldBe(container.GetInstance<ReadModel.Repository>().Id);
    }
}