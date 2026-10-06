using HerrGeneral.DDD;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.GenericKey;

public readonly record struct CustomerId(Guid Value);

public record CustomerCreated(string Name, Guid SourceCommandId, CustomerId AggregateId)
    : IDomainEvent<CustomerAggregate, CustomerId>
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class CustomerAggregate : Aggregate<CustomerAggregate, CustomerId>
{
    public string Name { get; }

    public CustomerAggregate(CustomerId id, string name, Guid commandId) : base(id)
    {
        Name = name;
        Emit(new CustomerCreated(name, commandId, Id));
    }
}

public record CreateCustomer(CustomerId CustomerId, string Name) : Create<CustomerAggregate, CustomerId>
{
    public class Handler : ICreateHandler<CustomerAggregate, CreateCustomer, CustomerId>
    {
        public CustomerAggregate Handle(CreateCustomer command, CustomerId aggregateId) =>
            new(command.CustomerId, command.Name, command.Id);
    }
}
