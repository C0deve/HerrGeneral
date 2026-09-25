namespace HerrGeneral.Test.Pipeline;

public class PipelineExecutionOrderTests(ITestOutputHelper output)
{
    public record ExecuteOrderCommand(Guid OrderId, string CustomerEmail);

    public record OrderPlacedEvent(Guid OrderId, string CustomerEmail);
    public record InventoryReservedEvent(Guid OrderId);

    public class ExecutionLog
    {
        public List<string> Order { get; } = [];
    }

    public class FakeUnitOfWork(ExecutionLog log) : IUnitOfWork
    {
        public void Start() => log.Order.Add("1:UnitOfWork:Start");
        public void Commit() => log.Order.Add("4:UnitOfWork:Commit");
        public void RollBack() => log.Order.Add("UnitOfWork:RollBack");
        public void Dispose() => log.Order.Add("UnitOfWork:Dispose");
    }

    public class OrderCommandHandler(ExecutionLog log) : ICommandHandler<ExecuteOrderCommand, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(ExecuteOrderCommand command)
        {
            log.Order.Add("2:CommandHandler:Handle");
            return ([new OrderPlacedEvent(command.OrderId, command.CustomerEmail)], Unit.Default);
        }
    }

    // In-transaction write-side handler (generates cascading event)
    public class OrderPlacedWriteSideHandler(ExecutionLog log) : IEventHandler<OrderPlacedEvent>
    {
        public IReadOnlyList<object> Handle(OrderPlacedEvent @event)
        {
            log.Order.Add("3a:WriteSide:ReserveInventory");
            return [new InventoryReservedEvent(@event.OrderId)];
        }
    }

    // In-transaction synchronous projection handler
    public class SyncOrderProjection(ExecutionLog log) : IHandleSyncProjection<OrderPlacedEvent>
    {
        public void Handle(OrderPlacedEvent @event) =>
            log.Order.Add("3b:SyncProjection:InsertOrderRecord");
    }

    // Post-transaction projection handler (eventual consistency)
    public class PostOrderProjection(ExecutionLog log) : IHandlePostProjection<OrderPlacedEvent>
    {
        public void Handle(OrderPlacedEvent @event) =>
            log.Order.Add("5a:PostProjection:UpdateAnalyticsView");
    }

    // Post-transaction side effect handler (external email / integration)
    public class SendConfirmationEmailSideEffect(ExecutionLog log) : IHandleSideEffect<OrderPlacedEvent>
    {
        public void Handle(OrderPlacedEvent @event) =>
            log.Order.Add("5b:SideEffect:SendEmailNotification");
    }

    [Fact]
    public async Task Validate_exact_pipeline_execution_order_across_transaction_boundary()
    {
        var log = new ExecutionLog();
        var uow = new FakeUnitOfWork(log);

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(log)
            .AddSingleton<IUnitOfWork>(uow)
            .AddSingleton<OrderCommandHandler>()
            .AddSingleton<OrderPlacedWriteSideHandler>()
            .AddSingleton<SyncOrderProjection>()
            .AddSingleton<PostOrderProjection>()
            .AddSingleton<SendConfirmationEmailSideEffect>()
            .AddHerrGeneral(cfg => cfg
                .ScanWriteSideOn(typeof(OrderCommandHandler).Assembly, typeof(OrderCommandHandler).Namespace!)
                .ScanSyncProjectionsOn(typeof(SyncOrderProjection).Assembly, typeof(SyncOrderProjection).Namespace!)
                .ScanPostProjectionsOn(typeof(PostOrderProjection).Assembly, typeof(PostOrderProjection).Namespace!)
                .ScanSideEffectsOn(typeof(SendConfirmationEmailSideEffect).Assembly, typeof(SendConfirmationEmailSideEffect).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var result = await mediator.Send(new ExecuteOrderCommand(Guid.NewGuid(), "customer@test.com"));

        result.IsSuccess.ShouldBeTrue();

        var expectedOrder = new[]
        {
            "1:UnitOfWork:Start",
            "2:CommandHandler:Handle",
            "3a:WriteSide:ReserveInventory",
            "3b:SyncProjection:InsertOrderRecord",
            "4:UnitOfWork:Commit",
            "UnitOfWork:Dispose",
            "5a:PostProjection:UpdateAnalyticsView",
            "5b:SideEffect:SendEmailNotification"
        };

        log.Order.ShouldBe(expectedOrder);
    }
}
