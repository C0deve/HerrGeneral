using HerrGeneral.Core.DDD;
using HerrGeneral.Core.Registration;
using HerrGeneral.Test.Extension;
using HerrGeneral.WriteSide.DDD.Test.Data;
using Lamar;
using Shouldly;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class RegisterDynamicHandlersShould
{
    private readonly Container _container;

    public RegisterDynamicHandlersShould(ITestOutputHelper output)
    {
        _container = new Container(cfg =>
        {
            cfg.AddHerrGeneralTestLogger(output);
            cfg.ForSingletonOf<IAggregateRepository<Person>>().Use<PersonRepository>();
            cfg.For<IAggregateFactory<Person>>().Use<DefaultAggregateFactory<Person>>();
            cfg
                .UseHerrGeneral(scanner =>
                    scanner.OnWriteSide(typeof(Person).Assembly, typeof(Person).Namespace!))
                .RegisterDynamicHandlers(typeof(AChangeCommandWithoutHandler).Assembly);
        });
    }

    [Fact]
    public void RegisterDynamicHandlersForChangeAggregateCommandWithoutHandler() => 
        _container.GetAllInstances<ICommandHandler<AChangeCommandWithoutHandler, Unit>>().Count.ShouldBe(1);
    
    [Fact]
    public void NotRegisterDynamicHandlersForChangeAggregateCommandWithHandler() => 
        _container.GetAllInstances<ICommandHandler<AddFriend, Unit>>().Count.ShouldBe(1);
    
    [Fact]
    public void RegisterDynamicHandlersForCreateAggregateCommandWithoutHandler() => 
        _container.GetAllInstances<ICommandHandler<ACreateCommandWithoutHandler, Guid>>().Count.ShouldBe(1);
    
    [Fact]
    public void NotRegisterDynamicHandlersForCreateAggregateCommandWithHandler() => 
        _container.GetAllInstances<ICommandHandler<CreatePerson, Guid>>().Count.ShouldBe(1);

}