using HerrGeneral.DDD;

namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.GenericKey;

public record OrderCreated(string OrderNumber, string Title, Guid SourceCommandId, string AggregateId)
    : IDomainEvent<OrderAggregate, string>
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}

public record OrderTitleChanged(string NewTitle, Guid SourceCommandId, string AggregateId)
    : IDomainEvent<OrderAggregate, string>
{
    public DateTime DateTimeEventOccurred { get; } = DateTime.Now;
    public Guid EventId { get; } = Guid.NewGuid();
}

public class OrderAggregate : Aggregate<OrderAggregate, string>
{
    public string Title { get; private set; }

    public OrderAggregate(string id, string title, Guid commandId) : base(id)
    {
        Title = title;
        Emit(new OrderCreated(id, title, commandId, Id));
    }

    public OrderAggregate(CreateOrderNoHandler command, string aggregateId)
        : this(aggregateId, command.Title, command.Id)
    {
    }

    public OrderAggregate ChangeTitle(string newTitle, Guid commandId)
    {
        Title = newTitle;
        return Emit(new OrderTitleChanged(newTitle, commandId, Id));
    }

    public OrderAggregate EmitWithMismatchedId(string wrongId, Guid commandId) =>
        Emit(new OrderTitleChanged(Title, commandId, wrongId));

    internal OrderAggregate Execute(ChangeOrderTitleNoHandler command) =>
        ChangeTitle(command.NewTitle, command.Id);
}

public record CreateOrder(string OrderNumber, string Title) : Create<OrderAggregate, string>
{
    public class Handler : ICreateHandler<OrderAggregate, CreateOrder, string>
    {
        public OrderAggregate Handle(CreateOrder command, string aggregateId) =>
            new(command.OrderNumber, command.Title, command.Id);
    }
}

public record CreateOrderNoHandler(string Title) : Create<OrderAggregate, string>, INoHandlerCreate<OrderAggregate>;

public record ChangeOrderTitle(string OrderNumber, string NewTitle) : Change<OrderAggregate, string>(OrderNumber)
{
    public class Handler : IChangeHandler<OrderAggregate, ChangeOrderTitle, string>
    {
        public OrderAggregate Handle(OrderAggregate aggregate, ChangeOrderTitle command) =>
            aggregate.ChangeTitle(command.NewTitle, command.Id);
    }
}

public record ChangeOrderTitleNoHandler(string OrderNumber, string NewTitle)
    : Change<OrderAggregate, string>(OrderNumber), INoHandlerChange<OrderAggregate>;
