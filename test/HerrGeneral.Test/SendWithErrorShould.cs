using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Data.ReadSide;
using HerrGeneral.Test.Data.WriteSide;
using Lamar;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Send;

public class SendWithErrorShould
{
    private readonly Container _container;

    public SendWithErrorShould(ITestOutputHelper output)
    {
        _container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);

            cfg.ForSingletonOf<Dependency>().Use(new Dependency());
            cfg.ForSingletonOf<ReadModel>().Use(new ReadModel());

            cfg.UseHerrGeneral(scanner =>
                scanner
                    .OnWriteSide(typeof(Ping).Assembly, typeof(Ping).Namespace!)
                    .OnReadSide(typeof(Ping).Assembly, typeof(ReadModel).Namespace!));
        });
    }


    [Fact]
    public async Task Return_result_failure_on_domain_error_thrown_from_command_handler() => 
        await new PingWithFailureInCommandHandler()
            .Send(_container, false)
            .ShouldHaveDomainErrorOfType<PingError>();

    [Fact]
    public async Task Return_result_failure_on_domain_error_thrown_from_event_handler() =>
        await new PingWithFailureInEventHandler()
            .Send(_container, false)
            .ShouldHaveDomainErrorOfType<PingError>();
    
    [Fact]
    public async Task Return_result_failure_on_panic_exception() => 
        await new PingWithPanicException()
            .Send(_container, false)
            .ShouldHavePanicExceptionOfType<SomePanicException>();
    [Fact]
    public async Task Return_result_failure_on_panic_exception_from_read_side() => 
        await new PingWithFailureInReadSideEventHandler()
            .Send(_container, false)
            .ShouldHavePanicExceptionOfType<SomePanicException>();
}