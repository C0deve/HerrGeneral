using HerrGeneral.Exception;
using HerrGeneral.Test.Data.WithMapping.WriteSide;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.DomainExceptionMapper.Test;

public class DomainExceptionMapperShould
{
    [Fact]
    public void MapDomainException() =>
        new Core.WriteSide.DomainExceptionMapper(typeof(MyDomainException))
            .Map(new PingError(),
                e => new DomainException(e),
                e => e)
            .ShouldBeOfType<DomainException>();

    [Fact]
    public void MapPanicException() =>
        new Core.WriteSide.DomainExceptionMapper(typeof(MyDomainException))
            .Map(new AnotherException(),
                e => new DomainException(e),
                e => new MappedException(e))
            .ShouldBeOfType<MappedException>();

    private class MappedException(System.Exception exception) : System.Exception("a mapped exception", exception);

    private class AnotherException : System.Exception;
}