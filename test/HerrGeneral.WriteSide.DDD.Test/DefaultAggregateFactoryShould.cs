using HerrGeneral.DDD;
using Shouldly;

namespace HerrGeneral.WriteSide.DDD.Test;

public class DefaultAggregateFactoryShould
{
    private record SampleCreate(string Name) : Create<StandardCtorAggregate, string>;
    private class StandardCtorAggregate : Aggregate<StandardCtorAggregate, string>
    {
        public string Name { get; }
        public StandardCtorAggregate(SampleCreate command, string aggregateId) : base(aggregateId)
        {
            Name = command.Name;
        }
    }

    private record InvertedCreate(string Name) : Create<InvertedCtorAggregate, string>;
    private class InvertedCtorAggregate : Aggregate<InvertedCtorAggregate, string>
    {
        public string Name { get; }
        public InvertedCtorAggregate(string aggregateId, InvertedCreate command) : base(aggregateId)
        {
            Name = command.Name;
        }
    }

    private record KeyOnlyCreate : Create<KeyOnlyCtorAggregate, int>;
    private class KeyOnlyCtorAggregate : Aggregate<KeyOnlyCtorAggregate, int>
    {
        public KeyOnlyCtorAggregate(int aggregateId) : base(aggregateId)
        {
        }
    }

    private record CommandOnlyCreate(string Title) : Create<CommandOnlyCtorAggregate, Guid>;
    private class CommandOnlyCtorAggregate : Aggregate<CommandOnlyCtorAggregate, Guid>
    {
        public string Title { get; }
        public CommandOnlyCtorAggregate(CommandOnlyCreate command) : base(Guid.NewGuid())
        {
            Title = command.Title;
        }
    }

    private record UnmatchedCreate : Create<UnmatchedCtorAggregate, string>;
    private class UnmatchedCtorAggregate : Aggregate<UnmatchedCtorAggregate, string>
    {
        public UnmatchedCtorAggregate(int invalidParam) : base("id")
        {
        }
    }

    [Fact]
    public void InstantiateWithStandardConstructor()
    {
        var factory = new DefaultAggregateFactory<StandardCtorAggregate, string>();
        var agg = factory.Create(new SampleCreate("Test"), "KEY-1");
        agg.Id.ShouldBe("KEY-1");
        agg.Name.ShouldBe("Test");
    }

    [Fact]
    public void InstantiateWithInvertedConstructor()
    {
        var factory = new DefaultAggregateFactory<InvertedCtorAggregate, string>();
        var agg = factory.Create(new InvertedCreate("Test"), "KEY-2");
        agg.Id.ShouldBe("KEY-2");
        agg.Name.ShouldBe("Test");
    }

    [Fact]
    public void InstantiateWithKeyOnlyConstructor()
    {
        var factory = new DefaultAggregateFactory<KeyOnlyCtorAggregate, int>();
        var agg = factory.Create(new KeyOnlyCreate(), 42);
        agg.Id.ShouldBe(42);
    }

    [Fact]
    public void InstantiateWithCommandOnlyConstructor()
    {
        var factory = new DefaultAggregateFactory<CommandOnlyCtorAggregate, Guid>();
        var agg = factory.Create(new CommandOnlyCreate("Hello"), Guid.NewGuid());
        agg.Title.ShouldBe("Hello");
    }

    [Fact]
    public void ThrowMissingMethodExceptionWhenNoCompatibleConstructor()
    {
        var factory = new DefaultAggregateFactory<UnmatchedCtorAggregate, string>();
        Should.Throw<MissingMethodException>(() => factory.Create(new UnmatchedCreate(), "KEY-3"));
    }
}
