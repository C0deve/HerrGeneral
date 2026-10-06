using HerrGeneral.DDD;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.GenericKey;

public record CounterCreated(int InitialValue, Guid SourceCommandId, int AggregateId)
    : IDomainEvent<CounterAggregate, int>
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}

public record CounterIncremented(int NewValue, Guid SourceCommandId, int AggregateId)
    : IDomainEvent<CounterAggregate, int>
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class CounterAggregate : Aggregate<CounterAggregate, int>
{
    public int Value { get; private set; }

    public CounterAggregate(int id, int value, Guid commandId) : base(id)
    {
        Value = value;
        Emit(new CounterCreated(value, commandId, Id));
    }

    public CounterAggregate Increment(Guid commandId)
    {
        Value++;
        return Emit(new CounterIncremented(Value, commandId, Id));
    }
}

public record CreateCounter(int CounterId, int InitialValue) : Create<CounterAggregate, int>
{
    public class Handler : ICreateHandler<CounterAggregate, CreateCounter, int>
    {
        public CounterAggregate Handle(CreateCounter command, int aggregateId) =>
            new(command.CounterId, command.InitialValue, command.Id);
    }
}

public record IncrementCounter(int CounterId) : Change<CounterAggregate, int>(CounterId)
{
    public class Handler : IChangeHandler<CounterAggregate, IncrementCounter, int>
    {
        public CounterAggregate Handle(CounterAggregate aggregate, IncrementCounter command) =>
            aggregate.Increment(command.Id);
    }
}
