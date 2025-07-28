using HerrGeneral.Core;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test;
using HerrGeneral.Test.Data.ReadSide;
using HerrGeneral.Test.Data.WriteSide;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Send;

public class SendWithErrorShould
{
    private readonly Mediator _mediator;

    public SendWithErrorShould(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<Dependency>(new Dependency())
            .AddSingleton(new ReadModel())
            .UseHerrGeneral(x =>
                x
                    .UseWriteSideAssembly(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .UseReadSideAssembly(typeof(Ping).Assembly, typeof(ReadModel).Namespace!)
                    .UseDomainException<MyDomainException>()
                    .MapCommandHandler<CommandBase, ILocalCommandHandler<CommandBase>, MyResult<Unit>>(result => result.Events)
                    .MapEventHandlerOnWriteSide<EventBase, Test.Data.WriteSide.ILocalEventHandler<EventBase>>()
                    .MapEventHandlerOnReadSide<EventBase, HerrGeneral.Test.Data.ReadSide.ILocalEventHandler<EventBase>>()
            );
        
        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<Mediator>();
    }


    [Fact]
    public async Task Return_result_failure_on_domain_error_thrown_from_command_handler() =>
        await new PingWithFailureInCommandHandler()
            .SendFrom(_mediator)
            .ShouldFailWithDomainErrorOfType<PingError>();

    [Fact]
    public async Task Return_result_failure_on_domain_error_thrown_from_event_handler() =>
        await new PingWithFailureInEventHandler()
            .SendFrom(_mediator)
            .ShouldFailWithDomainErrorOfType<PingError>();

    [Fact]
    public async Task Return_result_failure_on_panic_exception() =>
        await new PingWithPanicException()
            .SendFrom(_mediator)
            .ShouldFailWithPanicExceptionOfType<SomePanicException>();

    [Fact]
    public async Task Return_result_failure_on_panic_exception_from_read_side() =>
        await new PingWithFailureInReadSideEventHandler()
            .SendFrom(_mediator)
            .ShouldFailWithPanicExceptionOfType<SomePanicException>();
}