using HerrGeneral.Core.Error;
using HerrGeneral.Registration;
using HerrGeneral.Test;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task Resolve_main_command_handler_with_mapping()
    {
        var services = new ServiceCollection()
            .AddSingleton<Dependency>()
            .AddHerrGeneralTestLogger(output)
            .AddHerrGeneral(configuration =>
            {
                configuration
                    .RegisterCommandHandler<Ping, ICommandHandler<Ping>>()
                    .ScanWriteSideOn(typeof(PingHandler).Assembly, typeof(PingHandler).Namespace!);
                
                return configuration;
            });

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
            .AddSingleton<Dependency>(new Dependency())
            .AddHerrGeneral(scanner =>
                scanner
                    .ScanWriteSideOn(typeof(PingHandler).Assembly, "empty.namespace"));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<Mediator>();

        await Should.ThrowAsync<MissingCommandHandlerRegistrationException>(async () => await mediator.Send(new Ping()));
    }
}