using HerrGeneral.Core;

namespace HerrGeneral.Tracing;

/// <summary>
/// Types are named in traces and messages by <c>GetFriendlyName</c>: nested types keep their declaring types,
/// so that types sharing a short name ("Command", "Handler", "Service") can be told apart.
/// </summary>
public class FriendlyNameShould
{
    private static class CreateOrder
    {
        public class Command;

        public class Handler<TKey>;
    }

    private class Repository<TItem>
    {
        public class Cache<TKey>;
    }

    private class Plain;

    [Fact]
    public void KeepTheShortNameOfATopLevelType() =>
        typeof(string).GetFriendlyName().ShouldBe("String");

    [Fact]
    public void PrefixANestedTypeByItsDeclaringTypes() =>
        typeof(CreateOrder.Command).GetFriendlyName().ShouldBe("FriendlyNameShould.CreateOrder.Command");

    [Fact]
    public void TellApartNestedTypesSharingAShortName() =>
        typeof(CreateOrder.Command).GetFriendlyName().ShouldNotBe(typeof(Plain).GetFriendlyName());

    [Fact]
    public void WriteOutTheGenericArgumentsOfATopLevelType() =>
        typeof(Dictionary<string, List<int>>).GetFriendlyName().ShouldBe("Dictionary<String,List<Int32>>");

    [Fact]
    public void WriteOutTheGenericArgumentsOfANestedType() =>
        typeof(CreateOrder.Handler<Guid>).GetFriendlyName().ShouldBe("FriendlyNameShould.CreateOrder.Handler<Guid>");

    [Fact]
    public void GiveEachDeclaringTypeItsOwnGenericArguments() =>
        typeof(Repository<string>.Cache<int>).GetFriendlyName().ShouldBe("FriendlyNameShould.Repository<String>.Cache<Int32>");

    [Fact]
    public void NameTheParametersOfAnOpenGenericType() =>
        typeof(Repository<>.Cache<>).GetFriendlyName().ShouldBe("FriendlyNameShould.Repository<TItem>.Cache<TKey>");
}
