using HerrGeneral.DDD;
using HerrGeneral.DDD.Exception;
using HerrGeneral.WriteSide.DDD.Test.Data;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.GenericKey;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace HerrGeneral.WriteSide.DDD.Test;

public class GenericKeyAggregateShould
{
    private readonly Mediator _mediator;
    private readonly IServiceProvider _container;

    public GenericKeyAggregateShould(ITestOutputHelper output)
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<IAggregateRepository<OrderAggregate, string>, Repository<OrderAggregate, string>>()
            .AddSingleton<IAggregateRepository<CounterAggregate, int>, Repository<CounterAggregate, int>>()
            .AddSingleton<IAggregateRepository<CustomerAggregate, CustomerId>, Repository<CustomerAggregate, CustomerId>>()
            .AddHerrGeneral(configuration =>
                configuration
                    .ScanWriteSideOn(typeof(OrderAggregate).Assembly, "HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.GenericKey")
            );

        _container = services.BuildServiceProvider();
        _mediator = _container.GetRequiredService<Mediator>();
    }

    [Fact]
    public async Task CreateStringKeyAggregateWithExplicitHandler()
    {
        var result = await new CreateOrder("ORD-001", "My Order").SendFrom<OrderAggregate, string>(_mediator).ShouldSuccess();
        result.ShouldBe("ORD-001");

        var repo = _container.GetRequiredService<IAggregateRepository<OrderAggregate, string>>();
        var order = repo.Get("ORD-001");
        order.Title.ShouldBe("My Order");
        order.Id.ShouldBe("ORD-001");
    }

    [Fact]
    public async Task ChangeStringKeyAggregateWithExplicitHandler()
    {
        await new CreateOrder("ORD-002", "Initial Title").SendFrom<OrderAggregate, string>(_mediator).ShouldSuccess();

        await new ChangeOrderTitle("ORD-002", "Updated Title").SendFrom<OrderAggregate, string>(_mediator).ShouldSuccess();

        var repo = _container.GetRequiredService<IAggregateRepository<OrderAggregate, string>>();
        var order = repo.Get("ORD-002");
        order.Title.ShouldBe("Updated Title");
    }

    [Fact]
    public async Task CreateStringKeyAggregateWithDynamicHandler()
    {
        var result = await new CreateOrderNoHandler("Dynamic Order").SendFrom<OrderAggregate, string>(_mediator).ShouldSuccess();
        result.ShouldNotBeNullOrWhiteSpace();

        var repo = _container.GetRequiredService<IAggregateRepository<OrderAggregate, string>>();
        var order = repo.Get(result);
        order.Title.ShouldBe("Dynamic Order");
    }

    [Fact]
    public async Task ChangeStringKeyAggregateWithDynamicHandler()
    {
        var created = await new CreateOrderNoHandler("Title Before").SendFrom<OrderAggregate, string>(_mediator).ShouldSuccess();

        await new ChangeOrderTitleNoHandler(created, "Title After").SendFrom<OrderAggregate, string>(_mediator).ShouldSuccess();

        var repo = _container.GetRequiredService<IAggregateRepository<OrderAggregate, string>>();
        var order = repo.Get(created);
        order.Title.ShouldBe("Title After");
    }

    [Fact]
    public async Task CreateAndIncrementIntKeyAggregate()
    {
        var createResult = await new CreateCounter(42, 10).SendFrom<CounterAggregate, int>(_mediator).ShouldSuccess();
        createResult.ShouldBe(42);

        await new IncrementCounter(42).SendFrom<CounterAggregate, int>(_mediator).ShouldSuccess();

        var repo = _container.GetRequiredService<IAggregateRepository<CounterAggregate, int>>();
        var counter = repo.Get(42);
        counter.Value.ShouldBe(11);
    }

    [Fact]
    public async Task CreateStronglyTypedRecordKeyAggregate()
    {
        var customerId = new CustomerId(Guid.NewGuid());
        var result = await new CreateCustomer(customerId, "Alice").SendFrom<CustomerAggregate, CustomerId>(_mediator).ShouldSuccess();
        result.ShouldBe(customerId);

        var repo = _container.GetRequiredService<IAggregateRepository<CustomerAggregate, CustomerId>>();
        var customer = repo.Get(customerId);
        customer.Name.ShouldBe("Alice");
    }

    [Fact]
    public void ThrowIdMismatchOnEventEmitForStringKey()
    {
        Should.Throw<IdMismatchOnEventEmit<OrderAggregate, string>>(() =>
            new OrderAggregate("ORD-999", "Test", Guid.NewGuid()).EmitWithMismatchedId("WRONG-ID", Guid.NewGuid())
        );
    }

    [Fact]
    public void ThrowAggregateNotFoundOnMissingStringKey()
    {
        var repo = _container.GetRequiredService<IAggregateRepository<OrderAggregate, string>>();
        var ex = Should.Throw<AggregateNotFound<OrderAggregate>>(() => repo.Get("NON_EXISTING"));
        ex.Message.ShouldContain("NON_EXISTING");
    }
}
