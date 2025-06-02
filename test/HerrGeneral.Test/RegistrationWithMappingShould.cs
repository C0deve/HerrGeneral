using HerrGeneral.Core;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.Registration;
using HerrGeneral.WriteSide;
using Lamar;
using Shouldly;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Registration.Test;

public class RegistrationWithMappingShould(ITestOutputHelper output)
{
    private record Ping;

    private interface ICommandHandler<in TCommand>
    {
        public IEnumerable<object> Handle(TCommand command);
    }
    
    private class PingHandler(Dependency dependency) : ICommandHandler<Ping>
    {
        public IEnumerable<object> Handle(Ping command)
        {
            dependency.Called = true;
            return  [];
        }
    }

    private class Dependency
    {
        public bool Called { get; set; }
    }
    
    [Fact]
    public async Task Resolve_main_handler_with_mapping()
    {
        var container = new Container(cfg =>
        {
            cfg.ForSingletonOf<Dependency>().Use(new Dependency());
            cfg.AddHerrGeneralTestLogger(output);

            cfg.UseHerrGeneral(configuration =>
            {
                configuration
                    .MapHandler<Ping, ICommandHandler<Ping>>()
                    .UseWriteSideAssembly(typeof(PingHandler).Assembly, typeof(PingHandler).Namespace!);
                
                return configuration;
            });
        });

        var mediator = container.GetInstance<Mediator>();

        var response = await mediator.Send(new Ping());

        response.ShouldBe(Result.Success());
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
                    .UseWriteSideAssembly(typeof(PingHandler).Assembly, "empty.namespace"));
        });

        var mediator = container.GetInstance<Mediator>();

        await Should.ThrowAsync<MissingCommandHandlerRegistrationException>(async () => await mediator.Send(new Ping()));
    }
}