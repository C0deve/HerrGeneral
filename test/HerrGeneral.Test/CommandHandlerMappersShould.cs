using HerrGeneral.Core.Error;
using Shouldly;
using HerrGeneral.Test.Data.WriteSide;
using Unit = HerrGeneral.WriteSide.Unit;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.HandlerMappers.Test;

public class CommandHandlerMappersShould
{
    [Fact]
    public void ThrowExceptionWhenNoHandlerRegistered() =>
        Should.Throw<MissingCommandHandlerMapperException>(() =>
        {
            var sut = new Core.WriteSide.CommandHandlerMappings();
            sut.GetFromCommand(new Ping());
        });
    
    [Fact]
    public void ThrowExceptionWhenHandlerInterfaceIsNotGeneric() =>
        Should.Throw<HandlerTypeMustBeGenericMappingDefinitionException>(() =>
        {
            var sut = new Core.WriteSide.CommandHandlerMappings();
            sut.AddMapping<object, object, bool>(o => [o]);
        });

    [Fact]
    public void AddMultipleMappings()
    {
        var sut = new Core.WriteSide.CommandHandlerMappings();
        sut.AddMapping<CommandBase,
            ILocalCommandHandler<CommandBase>,
            MyResult<Unit>>(x => x.Events);

        sut.AddMapping<CommandBase,
            ILocalCommandHandler<CommandBase, Guid>,
            MyResult<Guid>,
            Guid>(
            x => x.Events,
            x => x.Result);
    }
    
    [Fact]
    public void ReturnMapperFromCommandAndResultType()
    {
        var sut = new Core.WriteSide.CommandHandlerMappings();
        sut.AddMapping<CommandBase,
            ILocalCommandHandler<CommandBase, Guid>,
            MyResult<Guid>,
            Guid>(
            x => x.Events,
            x => x.Result);
        
        sut.AddMapping<CommandBase,
            ILocalCommandHandler<CommandBase, int>,
            MyResult<int>,
            int>(
            x => x.Events,
            x => x.Result);
        
        sut.AddMapping<CommandBase,
            ILocalCommandHandler<CommandBase>,
            MyResult<Unit>>(x => x.Events);
        
        sut
            .GetFromCommand(new PingWithReturnValue(), typeof(int))
            .ShouldNotBeNull();
    }
}